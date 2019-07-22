namespace SanAndreasUnity.Importing.Items.Definitions
{
    [Section("node")]
    public class NodeDef : Definition, IObjectDefinition
    {
        public readonly int NodeId;

        int IObjectDefinition.Id
        {
            get { return NodeId; }
        }

        public readonly string Position;
        public readonly int LinkId;
        public readonly int AreaId;
        public readonly int PathWidth;
        public readonly int NodeType;
        public readonly int Flags;  // doesn't seem to be read by the game

        public NodeDef(string line)
            : base(line)
        {
            NodeId = GetInt(0);
            Position = GetString(1);
            LinkId = GetInt(2);
            AreaId = GetInt(3);
            PathWidth = GetInt(4);
            NodeType = GetInt(5);
            Flags = GetInt(6);
        }
    }
}