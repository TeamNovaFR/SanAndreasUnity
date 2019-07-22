using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Items.Definitions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;

namespace SanAndreasUnity.Importing.Vehicles
{
    public class PathNode
    {

        public static void Load(string path)
        {
            var paths = Directory.GetFiles(path, "NODES*.DAT");
            foreach (string _path in paths)
            {
                Debug.Log(_path);
                // debug
                var file = new ItemFile<Definition>(_path);

                uint numberOfNodes = 0;
                uint numberOfVehNodes;
                uint numberOfPedNodes;
                uint numberOfNaviNodes;
                uint numberOfLinks;

                // Reading the header of the file
                using (BinaryReader reader = new BinaryReader(File.Open(_path, FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    numberOfNodes = reader.ReadUInt32();
                    numberOfVehNodes = reader.ReadUInt32();
                    numberOfPedNodes = reader.ReadUInt32();
                    numberOfNaviNodes = reader.ReadUInt32();
                    numberOfLinks = reader.ReadUInt32();

                    Debug.Log("numberOfNodes : " + numberOfNodes);
                    Debug.Log("numberOfVehNodes : " + numberOfVehNodes);
                    Debug.Log("numberOfPedNodes : " + numberOfPedNodes);
                    Debug.Log("numberOfNaviNodes : " + numberOfNaviNodes);
                    Debug.Log("numberOfLinks : " + numberOfLinks);

                    for (int i = 0; i < numberOfNodes; i++)
                    {
                        uint memoryAddress;
                        uint unknownZero;
                        int[] pos = new int[3];

                        int unknownVar;

                        uint linkId;
                        uint areaId;
                        uint nodeId;
                        uint pathWidth;
                        uint nodeType;
                        uint flags;

                        // read here !
                        memoryAddress = reader.ReadUInt32(); // works
                        unknownZero = reader.ReadUInt32(); // works
                        pos[0] = reader.ReadInt16() / 8;
                        pos[1] = reader.ReadInt16() / 8;
                        pos[2] = reader.ReadInt16() / 8;
                        unknownVar = reader.ReadInt16();
                        linkId = reader.ReadUInt16(); // works
                        areaId = reader.ReadUInt16(); // works
                        nodeId = reader.ReadUInt16(); // works
                        pathWidth = reader.ReadByte();
                        nodeType = reader.ReadByte();
                        flags = reader.ReadUInt32();

                        Debug.Log(memoryAddress);
                        Debug.Log(unknownZero);
                        Debug.Log(pos[0]);
                        Debug.Log(pos[1]);
                        Debug.Log(pos[2]);
                        Debug.Log(unknownVar);
                        Debug.Log(linkId);
                        Debug.Log(areaId);
                        Debug.Log(nodeId);
                        Debug.Log(pathWidth);
                        Debug.Log(nodeType);
                        Debug.Log(flags);

                        
                    }
                    break;

                }

            }
        }

        private readonly int[][] _vals;

        public int[] this[int index]
        {
            get { return _vals[index]; }
        }
    }
}