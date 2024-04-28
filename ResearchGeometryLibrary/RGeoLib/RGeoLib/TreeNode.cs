using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace RGeoLib
{
    public class TreeNode : IEnumerable<TreeNode>
    {
        //public Dictionary<string, TreeNode> _children =
        //                                new Dictionary<string, TreeNode>();

        public Dictionary<string, TreeNode> _children { get; set; }


        //public Dictionary<string, double> _value =
        //                        new Dictionary<string, double>();

        public int globalIterator = 0;

        public double split { get; set; }
        public double angle { get; set; }
        // merge_id can be the same, will then be used to combine divided areas
        public string merge_id;

        public NMesh treeNodeMesh;

        // ID has to be unique for every node
        public string ID { get; set; }
        public TreeNode Parent { get; private set; }

        public TreeNode(string id)
        {
            this.ID = id;
            this.split = 0;
            this.angle = 0;
            this.merge_id = "";

            this._children = new Dictionary<string, TreeNode>();
        }

        public TreeNode GetChild(string id)
        {
            return this._children[id];
        }

        public List<TreeNode> GetAllChildren()
        {
            
            List<TreeNode> childrenList = new List<TreeNode>(); 
            foreach (string key in this._children.Keys)
            {
                //Console.WriteLine(key);
            }

            //Console.WriteLine("----------------");

            foreach (string key in this._children.Keys)
            {
                TreeNode child = this._children[key];
                childrenList.Add(child);
            }
            return childrenList;

        }

        public void Add(TreeNode item)
        {
            if (item.Parent != null)
            {
                item.Parent._children.Remove(item.ID);
            }

            item.Parent = this;
            this._children.Add(item.ID, item);
        }

        

        public static int CountIDs(TreeNode tree, string idToSearch)
        {
            string treeString = TreeNode.BuildString(tree);
            int numExsistingID = Regex.Matches(treeString, idToSearch).Count;
            return numExsistingID;
        }
        public static void SubdivideTree(TreeNode tree, TreeNode item, List<int> subdivisionList)
        {
            // creates Parent and puts children into lower trees, divided by subdivision list
            //  
            //   e.g. tree X with 3 children abc, subdivided in [1,2]
            //
            //               x               x
            //              /|\    ->       / \
            //             a b c           p1  p2 
            //                            / \   \
            //                            a  b   c
            //

            List<TreeNode> childrenList = item.GetAllChildren();
            childrenList.ForEach(child => Console.WriteLine(child.ID));
            int numNewParents = subdivisionList.Count;

            //  check if number of children and number of subdivisions are compatible
            //int numSubdivisionNodes = 0;
            //for (int i= 0; i < subdivisionList.Count; i++)
            //{
            //    numSubdivisionNodes+=subdivisionList[i];
            // }

            //if (numSubdivisionNodes != numNewParents)
            //{
            //    return;
            //}

            // iterate through whole tree to see what number TreeParent is at
            string treeString = TreeNode.BuildString(tree);
            int numExsistingParents = Regex.Matches(treeString, "TreeParent").Count;
            tree.globalIterator = numExsistingParents;


            Console.WriteLine($"Subdivision of tree {item.ID} started");
            Console.WriteLine(childrenList.Count);
            Console.WriteLine($"Number of new Parents {numNewParents}");
            
            // go through subdivisionList
            for (int i = 0; i < (numNewParents); i++)
            {
                Console.WriteLine($"Parent{i}");
                
                
                TreeNode parentNode = new TreeNode($"{tree.globalIterator}_TreeParent_0");
                tree.globalIterator++;

                Console.WriteLine(subdivisionList[i]);

                for (int j = 0; j < subdivisionList[i]; j++)
                {
                    parentNode.Add(childrenList[0]);
                    childrenList.RemoveAt(0);
                    //item.Parent._children.Remove(currentChild.ID);
                }
                item.Add(parentNode);
            }
        }

        public IEnumerator<TreeNode> GetEnumerator()
        {
            return this._children.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public int Count
        {
            get { return this._children.Count; }
        }

        // Static methods
        public static TreeNode BuildTree(string tree)
        {
            var lines = tree.Split(new[] { Environment.NewLine },
                                   StringSplitOptions.RemoveEmptyEntries);

            var result = new TreeNode("TreeRoot");
            var list = new List<TreeNode> { result };

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                var indent = line.Length - trimmedLine.Length;

                var child = new TreeNode(trimmedLine);
                list[indent].Add(child);

                if (indent + 1 < list.Count)
                {
                    list[indent + 1] = child;
                }
                else
                {
                    list.Add(child);
                }
            }

            return result;
        }


        public static NMesh BuildNMesh(TreeNode tree)
        {
            // do same as build string, just with mesh

            List<NFace> tempFaces = tree.treeNodeMesh.faceList;

            NMesh outMesh = new NMesh(tempFaces);

            foreach (var child in tree._children)
            {
                List<NFace> temp2Face = child.Value.treeNodeMesh.faceList;
                outMesh.faceList.AddRange(temp2Face);

                if (child.Value.GetAllChildren().Count == 0)
                {

                }
                else
                {
                    NMesh tempMesh = BuildNMesh(tree._children[child.Key]);
                    outMesh.faceList.AddRange(tempMesh.faceList);
                }
            }
            return outMesh;
        }

        public static string BuildString(TreeNode tree)
        {
            var sb = new StringBuilder();

            BuildString(sb, tree, 0);

            return sb.ToString();
        }

        private static void BuildString(StringBuilder sb, TreeNode node, int depth)
        {
            sb.AppendLine(node.ID.PadLeft(node.ID.Length + depth));

            foreach (var child in node)
            {
                BuildString(sb, child, depth + 1);
            }
        }

        public static string BuildStringAllProperties(TreeNode tree)
        {
            var sb = new StringBuilder();

            BuildStringAll(sb, tree, 0);

            return sb.ToString();
        }

        private static void BuildStringAll(StringBuilder sb, TreeNode node, int depth)
        {
            sb.AppendLine(node.ID.PadLeft(node.ID.Length + depth));
            sb.AppendLine("split:".PadLeft(node.ID.Length + depth) + node.split.ToString());
            sb.AppendLine("angle:".PadLeft(node.ID.Length + depth) + node.angle.ToString());

            foreach (var child in node)
            {
                BuildStringAll(sb, child, depth + 1);
            }
        }

        public static void BuildValueTree(TreeNode inputTree)
        {
            // "2_Cluster_0 / 20_7_90"
            // "ID_SPLIT_ANLGE"

            // Update 220927 "Optional merge_id for merging"
            // "2_Cluster_0 / 20_7_90_2"
            // "ID_SPLIT_ANLGE_merge_id"

            // build lowest tier
            foreach (var node in inputTree._children)
            {
                TreeNode.BuildValueTree(inputTree._children[node.Key]);
                
                string[] angleSubs = node.Key.Split('_');
                double currentAngle = Convert.ToDouble(angleSubs[2]);
                double angleRad = RMath.ToRadians(currentAngle);
                node.Value.angle = angleRad;

                // 220927_EDIT
                // check if merge_id is provided
                if (angleSubs.Length > 3)
                {
                    string merge_id = angleSubs[3];
                    node.Value.merge_id = merge_id;
                }

                Console.WriteLine(node);
                List<TreeNode> currentChildren = node.Value.GetAllChildren();
                if (currentChildren.Count == 0)
                {
                    string[] subs = node.Key.Split('_');
                    // "2_Cluster_0 / 20_7_90"

                    // split cluster 
                    // if has no children must be bottom node, add split size
                    double doubleCurrentValue = Convert.ToDouble(subs[1]);
                    node.Value.split = doubleCurrentValue;
                    
                    Console.WriteLine(doubleCurrentValue);
                    Console.WriteLine(node.Value.split);
                }
                else 
                {
                    double aggregateValue = 0;
                    //Console.WriteLine($"NumChildren: {currentChildren.Count}");
                    for(int i = 0; i < currentChildren.Count; i++)
                    {
                        //Console.WriteLine($"value of children {currentChildren[i].value}");
                        aggregateValue += Convert.ToDouble(currentChildren[i].split);
                    }
                    Console.WriteLine(aggregateValue);
                 
                    node.Value.split = aggregateValue;
                }

                
            }
        }
        public static void PrintHierarchy(TreeNode inputTree)
        {
            foreach (var node in inputTree._children)
            {
                Console.WriteLine(node.Value.split.ToString());
                Console.WriteLine(node.Value.angle.ToString());

                if (node.Value.GetAllChildren().Count == 0)
                {

                } else
                {
                    PrintHierarchy(inputTree._children[node.Key]);
                }

                Console.WriteLine("------------");
            }
        }


        // JSON
        /*
         * public static void JsonOut(TreeNode inputTree)
        {
            foreach (var node in inputTree._children)
            {
                //Console.WriteLine(node.Value.split.ToString());
                //Console.WriteLine(node.Value.angle.ToString());
                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(node);
                Console.WriteLine(jsonString);

                if (node.Value.GetAllChildren().Count == 0)
                {

                }
                else
                {
                    JsonOut(inputTree._children[node.Key]);
                }

                Console.WriteLine("------------");
            }
        }
        */

        /*
        public override string ToString()
        {
            string outstring = "";

            foreach (var node in this._children)
            {
                //TreeNode.BuildValueTree(this._children[node.Key]);

                string[] angleSubs = node.Key.Split('_');
                double currentAngle = Convert.ToDouble(angleSubs[2]);
                double angleRad = RMath.ToRadians(currentAngle);
                node.Value.angle = angleRad;

                // 220927_EDIT
                // check if merge_id is provided
                if (angleSubs.Length > 3)
                {
                    string merge_id = angleSubs[3];
                    node.Value.merge_id = merge_id;
                }
                outstring += node + "\n";

                // Console.WriteLine(node);
                List<TreeNode> currentChildren = node.Value.GetAllChildren();
                if (currentChildren.Count == 0)
                {
                    string[] subs = node.Key.Split('_');
                    // "2_Cluster_0 / 20_7_90"

                    // split cluster 
                    // if has no children must be bottom node, add split size
                    double doubleCurrentValue = Convert.ToDouble(subs[1]);
                    node.Value.split = doubleCurrentValue;

                    //Console.WriteLine(doubleCurrentValue);
                    outstring += doubleCurrentValue + "\n";
                    outstring += node.Value.split + "\n";
                }
                else
                {
                    double aggregateValue = 0;
                    //Console.WriteLine($"NumChildren: {currentChildren.Count}");
                    for (int i = 0; i < currentChildren.Count; i++)
                    {
                        //Console.WriteLine($"value of children {currentChildren[i].value}");
                        aggregateValue += Convert.ToDouble(currentChildren[i].split);
                    }
                    outstring += aggregateValue + "\n";

                    node.Value.split = aggregateValue;
                }


            }

            return outstring;
        }
       
        */

        /*
        public static TreeNode TrimTree(TreeNode inputTree)
        {
            // 1 Create blank tree node
            
            // 2 Analyze input Tree node            
            TreeNode outTree = new TreeNode(inputTree.ID);
            List<TreeNode> listOfChildren = outTree.GetAllChildren();

            // 2 if at bottom of hierarchy
            if (listOfChildren[0].GetAllChildren().Count == 0)
            {
                // combine values of all tree nodes .values
                
                // delete all nodes in list of children

                // change .value of current item in dict
                return outTree;
            } 
            else 
            {

                // move to next node    
            }

            foreach (var child in inputTree)
            {
                Console.Write(inputTree._children);
                //Console.Write(child.ID);
            }
            return outTree;
        }
        */


        /*
         * 
         *             var tree = TreeNode.BuildTree(@"
Apt-B-01
 Cluster1
  18
  12
  15
 Cluster2
  10
  5
");
        Console.WriteLine(TreeNode.BuildString(tree));
         * 
         */

    }
}
