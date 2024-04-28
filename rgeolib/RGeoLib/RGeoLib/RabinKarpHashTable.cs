using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGeoLib
{
    public class RabinKarpHashTableVec<T> where T : Vec3d
    {
        // Define the hash function and the tolerance
        private Func<T, int> _hashFunc;
        private double _tolerance;

        // Define the hash table
        private Dictionary<int, List<T>> _table;

        public RabinKarpHashTableVec(double tolerance)
        {
            // Initialize the hash function and the tolerance
            _hashFunc = obj => obj.GetHashCode();
            _tolerance = tolerance;

            // Initialize the hash table
            _table = new Dictionary<int, List<T>>();
        }

        public void Add(T obj)
        {
            // Compute the hash code of the object
            int hashCode = _hashFunc(obj);
            // Add the object to the list of objects with the same hash code
            if (!_table.ContainsKey(hashCode))
            {
                _table[hashCode] = new List<T>();
            }
            _table[hashCode].Add(obj);
        }

        public bool Contains(T obj)
        {
            // Compute the hash code of the object
            int hashCode = _hashFunc(obj);
            // Check if the hash table contains an entry for the given hash code
            if (!_table.ContainsKey(hashCode))
            {
                return false;
            }
            // Check if the list of objects with the same hash code contains the object
            List<T> objects = _table[hashCode];
            return objects.Any(x => Vec3d.Distance(x, obj) <= _tolerance);
        }
    }

    public class RabinKarpHashTableLine<T> where T : NLine
    {
        // Define the hash function and the tolerance
        private Func<T, int> _hashFunc;
        private double _tolerance;

        // Define the hash table
        private Dictionary<int, List<T>> _table;

        public RabinKarpHashTableLine(double tolerance)
        {
            // Initialize the hash function and the tolerance
            _hashFunc = obj => obj.GetHashCode();
            _tolerance = tolerance;

            // Initialize the hash table
            _table = new Dictionary<int, List<T>>();
        }

        public void Add(T obj)
        {
            // Compute the hash code of the object
            int hashCode = _hashFunc(obj);
            // Add the object to the list of objects with the same hash code
            if (!_table.ContainsKey(hashCode))
            {
                _table[hashCode] = new List<T>();
            }
            _table[hashCode].Add(obj);
        }

        public bool Contains(T obj)
        {
            // Compute the hash code of the object
            int hashCode = _hashFunc(obj);
            // Check if the hash table contains an entry for the given hash code
            if (!_table.ContainsKey(hashCode))
            {
                return false;
            }
            // Check if the list of objects with the same hash code contains the object
            List<T> objects = _table[hashCode];
            return objects.Any(x => ((Vec3d.Distance(x.start, obj.start) <= _tolerance) && (Vec3d.Distance(x.end, obj.end) <= _tolerance)) || ((Vec3d.Distance(x.start, obj.end) <= _tolerance) && (Vec3d.Distance(x.end, obj.start) <= _tolerance)) );
        }
    }

}
