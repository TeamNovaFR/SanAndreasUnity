using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Items.Definitions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SanAndreasUnity.Importing.Vehicles
{
    public class PathNode
    {
        private static Dictionary<string, PathNode> _pathNodes;

        public static void Load(string path)
        {
            var file = new ItemFile<Definition>(path);

            Debug.LogFormat("Entries: {0}", file.GetItems<NodeDef>().Count());
        }

        private readonly NodeDef _def;
        private readonly int[][] _vals;

        public int[] this[int index]
        {
            get { return _vals[index]; }
        }
    }
}