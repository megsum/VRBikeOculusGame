using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MedRoad.Ant
{
    internal static class AntUtilFunctions
    {
        /// <summary>
        /// Find all subclasses of the given type and return an enumeration with the Type object
        /// for each subclass.
        /// </summary>
        /// <returns>An enumeration containing one Type object for every subclass of the given
        /// type.</returns>
        /// <typeparam name="T">The type to find subclasses of.</typeparam>
        internal static IEnumerable<Type> GetEnumerableOfType<T>() where T : class
        {
            // http://stackoverflow.com/a/6944605

            List<Type> objects = new List<Type>();
            foreach (Type type in Assembly.GetAssembly(typeof(T)).GetTypes()
                     .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
            {
                objects.Add(type);
            }
            return objects;
        }

        /// <summary>
        /// Calculates the difference between two (unsigned) byte counters that rollover from 255
        /// to 0.
        /// </summary>
        /// <param name="op1">The first operand (to subtract from).</param>
        /// <param name="op2">The second operand (to subtract).</param>
        /// <returns>The result as an integer in [0, 255].</returns>
        internal static int rolloverDiff(byte op1, byte op2)
        {
            int result = (op1 - op2) % 256;
            return result + ((result < 0) ? 256 : 0);
        }

        /// <summary>
        /// Calculates the difference between two unsigned short counters that rollover from 65535
        /// to 0.
        /// </summary>
        /// <param name="op1">The first operand (to subtract from).</param>
        /// <param name="op2">The second operand (to subtract).</param>
        /// <returns>The result as an integer in [0, 65535].</returns>
        internal static int rolloverDiff(ushort op1, ushort op2)
        {
            int result = (op1 - op2) % 65536;
            return result + ((result < 0) ? 65536 : 0);
        }

        /// <summary>
        /// Clamps the given integer value between the given minimum and maximum.
        /// </summary>
        /// <param name="value">The integer to clamp.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <returns>The vlaue, clamped.</returns>
        internal static int IntegerClamp(int value, int min, int max)
        {
            if (value < min)
                value = min;

            else if (value > max)
                value = max;

            return value;
        }
    }

    internal class ZeroSpeedCountBuffer
    {
        int[] counts;
        int pos = 0;
        bool wrappedAround = false;

        public ZeroSpeedCountBuffer(int size)
        {
            counts = new int[size];
        }

        public void Reset()
        {
            pos = 0;
            wrappedAround = false;
        }

        public void Add(int count)
        {
            counts[pos++] = count;

            if (pos >= counts.Length)
            {
                pos = 0;
                wrappedAround = true;
            }
        }

        public int Average()
        {
            if (wrappedAround)
                return CalculateAverage(counts.Length);
            else
                return CalculateAverage(pos);
        }

        private int CalculateAverage(int size)
        {
            if (size == 0)
                return 0;

            int sum = 0;
            for (int i = 0; i < size; i++)
                sum += counts[i];

            return (int) Math.Round( ((double)sum) / size);
        }
    }

}
