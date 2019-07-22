using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Behaviours.World;
using SanAndreasUnity.Importing.Animation;
using System.Linq;

namespace SanAndreasUnity.Behaviours.Peds.States
{

    public class VehicleEnteringState : BaseVehicleState
    {
        Coroutine m_coroutine;
        bool m_immediate = false;


        public override void OnBecameActive()
        {
            base.OnBecameActive();
            if (m_isServer) // clients will do this when vehicle gets assigned
                this.EnterVehicleInternal();
        }

        public override void OnBecameInactive()
        {
            // restore everything

            m_immediate = false;

            this.Cleanup();

            if (m_coroutine != null)
                StopCoroutine(m_coroutine);
            m_coroutine = null;

            base.OnBecameInactive();
        }

        protected override void OnVehicleAssigned()
        {
            this.EnterVehicleInternal();
        }

        public bool TryEnterVehicle(Vehicle vehicle, Vehicle.SeatAlignment seatAlignment, bool immediate = false)
        {
            Net.NetStatus.ThrowIfNotOnServer();

            if (!this.CanEnterVehicle(vehicle, seatAlignment))
                return false;

            this.EnterVehicle(vehicle, seatAlignment, immediate);

            return true;
        }

        internal void EnterVehicle(Vehicle vehicle, Vehicle.SeatAlignment seatAlignment, bool immediate)
        {
            // first assign params
            this.CurrentVehicle = vehicle;
            this.CurrentVehicleSeatAlignment = seatAlignment;
            m_immediate = immediate;

            // switch state
            m_ped.SwitchState<VehicleEnteringState>();
        }

        void EnterVehicleInternal()
        {

            Vehicle vehicle = this.CurrentVehicle;
            Vehicle.Seat seat = this.CurrentVehicleSeat;
            bool immediate = m_immediate;


            BaseVehicleState.PreparePedForVehicle(m_ped, vehicle, seat);

            if (seat.IsDriver)
            {
                // TODO: this should be done when ped enters the car - or, it should be removed, because
                // vehicle should know if it has a driver
                vehicle.StartControlling();

                // if (m_isServer) {
                // 	var p = Net.Player.GetOwningPlayer(m_ped);
                // 	if (p != null)
                // 		Net.NetManager.AssignAuthority(vehicle.gameObject, p);
                // }
            }

            if (!vehicle.IsNightToggled && WorldController.IsNight)
                vehicle.IsNightToggled = true;
            else if (vehicle.IsNightToggled && !WorldController.IsNight)
                vehicle.IsNightToggled = false;


            m_coroutine = StartCoroutine(EnterVehicleAnimation(seat, immediate));

        }
        static bool isAdjusting, applyToParent;
        // Adjust root frame pos
        public static void AdjustRootFramePosition(Ped ped, Vector3 pos)
        {
            // we need to adjust local position of some bones - root frame needs to be 0.5 units below the ped

            var model = ped.PlayerModel;

            if (null == model.RootFrame)
                return;
            if (null == model.UnnamedFrame)
                return;

            // for some reason, y position always remains 0.25
            if(applyToParent)
                model.UnnamedFrame.transform.localPosition = pos;
            model.RootFrame.transform.localPosition = pos;

        }

        void LateUpdate()
        {
            if (isAdjusting)
                AdjustRootFramePosition(m_ped, Vector3.zero);
        }

        public enum DoorAlignment
        {
            None,
            RightFront,
            LeftFront,
            RightRear,
            LeftRear,
        }

        private DoorAlignment GetDoorAlignment(string frameName)
        {
            switch (frameName)
            {
                case "door_rf_dummy":
                    return DoorAlignment.RightFront;

                case "door_lf_dummy":
                    return DoorAlignment.LeftFront;

                case "door_rr_dummy":
                    return DoorAlignment.RightRear;

                case "door_lr_dummy":
                    return DoorAlignment.LeftRear;

                default:
                    return DoorAlignment.None;
            }
        }

        private System.Collections.IEnumerator EnterVehicleAnimation(Vehicle.Seat seat, bool immediate)
        {
            var animIndex = seat.IsLeftHand ? AnimIndex.GetInLeft : AnimIndex.GetInRight;
            var animOpenIndex = seat.IsLeftHand ? "CAR_open_LHS" : "CAR_open_RHS";
            var animCloseIndex = seat.IsLeftHand ? "car_rolldoor" : "car_rolldoor";
            var animSittedIndex = "CAR_sit";

            m_model.VehicleParentOffset = Vector3.Scale(m_model.GetAnim(AnimGroup.Car, animIndex).RootEnd, new Vector3(-1, -1, -1));
            

            if (!immediate)
            {
                var animState = m_model.PlayAnim("ped", animOpenIndex, PlayMode.StopAll);
                animState.wrapMode = WrapMode.Once;

                HingeJoint hinge = seat.door.GetComponent<HingeJoint>();
                var doorAlignment = GetDoorAlignment(seat.door.name);
                var limit = 90.0f * ((doorAlignment == DoorAlignment.LeftFront || doorAlignment == DoorAlignment.LeftRear) ? 1.0f : -1.0f);

                // wait until anim is finished or vehicle is destroyed
                while (animState != null && animState.enabled && this.CurrentVehicle != null && hinge.limits.min != limit)
                {
                    isAdjusting = true;
                    if (animState.normalizedTime > 0.4)
                        hinge.limits = new JointLimits { min = Mathf.Lerp(hinge.limits.min, limit, Time.deltaTime * 4), max = Mathf.Lerp(hinge.limits.min, limit + 0.1f, Time.deltaTime * 4), };
                    yield return new WaitForEndOfFrame();
                }
                isAdjusting = false;

                var animState2 = m_model.PlayAnim(AnimGroup.Car, animIndex, PlayMode.StopAll);
                animState2.wrapMode = WrapMode.Once;

                // wait until anim is finished or vehicle is destroyed
                while (animState2 != null && animState2.enabled && this.CurrentVehicle != null)
                {
                    yield return new WaitForEndOfFrame();
                }
                isAdjusting = true;
                applyToParent = true;

                var animState3 = m_model.PlayAnim("ped", animCloseIndex, PlayMode.StopAll);
                animState3.wrapMode = WrapMode.Once;

                //Importing.Conversion.Animation.RemovePositionCurves(animState3.clip, m_model.Frames);
                while (animState3 != null && animState3.enabled && this.CurrentVehicle != null && hinge.limits.max != 0)
                {

                    if (animState3.normalizedTime > 0.4)
                        hinge.limits = new JointLimits { min = Mathf.Lerp(hinge.limits.min, 0, Time.deltaTime * 4), max = Mathf.Lerp(hinge.limits.max, 0f, Time.deltaTime * 4), };
                    yield return new WaitForEndOfFrame();
                }
                
                hinge.limits = new JointLimits { min = 0, max = 0, };

                var animState4 = m_model.PlayAnim("ped", animSittedIndex, PlayMode.StopAll);

            }
            

            // check if vehicle is alive
            if (null == this.CurrentVehicle)
            {
                // vehicle destroyed in the meantime ? hmm... ped is a child of vehicle, so it should be
                // destroyed as well ?
                // anyway, switch to stand state
                if (m_isServer)
                    m_ped.SwitchState<StandState>();
                yield break;
            }
            isAdjusting = false;
            applyToParent = false;


            // ped now completely entered the vehicle

            // call method from VehicleSittingState - he will switch state
            if (m_isServer)
                m_ped.GetStateOrLogError<VehicleSittingState>().EnterVehicle(this.CurrentVehicle, seat);

        }

    }

}
