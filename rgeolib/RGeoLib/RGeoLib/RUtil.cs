using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGeoLib
{
    public class RUtil
    {
        // Permutation Util

        /*
         * Code Sample for program
         * 
            List<double> angleList = new List<double>() { 0.1 , 0.2, 0.3, 0.4 };

            List<int> tempL = new List<int>() { 0,1,2,3 };
            List<List<int>> tempLists = RUtil.Permute(tempL);

            
            Console.WriteLine("anglelist-Old------");
            angleList.ForEach(p => Console.WriteLine(p));

            List<List<int>> permListsCompiled = RUtil.GetPermutationLists(angleList.Count);
            Console.WriteLine("permlist--------");
            permListsCompiled[3].ForEach(p => Console.WriteLine(p));

            List<double> angleNewList = RUtil.PermuteWithList(angleList, permListsCompiled[3]);
            Console.WriteLine("anglelist--------");
            angleNewList.ForEach(p => Console.WriteLine(p));

            Console.WriteLine("--------");

        */

        public static List<double> PermuteWithList(List<double> inputList, List<int> permuationList)
        {
            List<double> result = new List<double>();
            for (int i = 0; i < permuationList.Count; i++)
            {
                result.Add(inputList[permuationList[i]]);
            }
            return result;
        }

        public static List<List<int>> GetPermutationLists(int listLength)
        {
            List<int> tempL = new List<int>();
            for (int i = 0; i < listLength; i++)
            {
                tempL.Add(i);
            }
            List<List<int>> tempLists = RUtil.Permute(tempL);

            return tempLists;
        }
        public static List<List<int>> Permute(List<int>nums)
        {
            var list = new List<List<int>>();
            return DoPermute(nums, 0, nums.Count - 1, list);
        }

        public static List<List<int>> DoPermute(List<int> nums, int start, int end, List<List<int>> list)
        {
            if (start == end)
            {
                // We have one of our possible n! solutions,
                // add it to the list.
                list.Add(new List<int>(nums));
            }
            else
            {
                for (var i = start; i <= end; i++)
                {
                    Swap(nums, start, i);
                    DoPermute(nums, start + 1, end, list);
                    Swap(nums, start, i);
                }
            }

            return list;
        }

        public static void Swap(List<int> nums, int a, int b)
        {
            int temp = nums[a];
            int temp2 = nums[b];
            nums[a] = temp2;
            nums[b] = temp;
        }


        // combination of lists
        public static List<string> GetAllPossibleCombos(List<List<string>> strings)
        {
            IEnumerable<string> combos = new[] { "" };

            foreach (var inner in strings)
            {
                combos = combos.SelectMany(r => inner.Select(x => r + x));
            }

            return combos.ToList();
        }



        // sort list by other list of any type
        public static List<T> SortListByOtherList<T, U>(List<T> listToSort, List<U> listToSortBy)
        {
            List<Tuple<U, T>> combinedList = new List<Tuple<U, T>>();
            for (int i = 0; i < listToSort.Count; i++)
            {
                combinedList.Add(Tuple.Create(listToSortBy[i], listToSort[i]));
            }

            combinedList.Sort((x, y) => Comparer<U>.Default.Compare(x.Item1, y.Item1));

            List<T> sortedList = new List<T>();
            foreach (var item in combinedList)
            {
                sortedList.Add(item.Item2);
            }

            return sortedList;
        }
        
        public static double cummulative(List<double> inputList)
        {
            double result = 0;

            for (int i = 0; i < inputList.Count; i++)
            {
                result += inputList[i];
            }
            return result;
        }
        public static double average(List<double> inputList)
        {
            double average = 0;

            for (int i = 0; i < inputList.Count; i++)
            {
                average += inputList[i];
            }
            average /= inputList.Count;

            return average;
        }

        public static bool checkForOne(List<double> inputList, double minValue)
        {
            bool smallerthan = false;
            for (int i = 0; i < inputList.Count; i++)
            {
                if (inputList[i] >= minValue)
                    smallerthan = true;
            }

            return smallerthan;
        }

        public static bool largerThan(List<double> inputList, double minValue)
        {
            bool smallerthan = true;
            for (int i = 0; i < inputList.Count; i++)
            {
                if (inputList[i] < minValue)
                    smallerthan = false;
            }

            return smallerthan;
        }
    }
}
