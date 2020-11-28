using System.Collections.Generic;

namespace OpalTests
{
    public static class NfaArray
    {
        public static List<int> Create() => new List<int>();

        public static List<int> Add(this List<int> array, int index, int state = 0, int left = -1, int match = -1, int right = -1)
        {
            array.Add(index);
            array.Add(state);
            array.Add(left);
            if (left != -1)
                array.Add(match);
            array.Add(right);
            return array;
        }

        public static List<int> None(this List<int> array, int index)
        {
            array.Add(index);
            array.Add(0);
            array.Add(-1);
            array.Add(-1);
            return array;
        }

        public static List<int> Epsilon1(this List<int> array, int index, int right) =>
            array.Epsilon1(index, 0, right);

        public static List<int> Epsilon1(this List<int> array, int index, int state, int right)
        {
            array.Add(index);
            array.Add(state);
            array.Add(-1);
            array.Add(right);
            return array;
        }


        public static List<int> Epsilon2(this List<int> array, int index, int left, int right) =>
            array.Epsilon2(index, 0, left, right);

        public static List<int> Epsilon2(this List<int> array, int index, int state, int left, int right)
        {
            array.Add(index);
            array.Add(state);
            array.Add(left);
            array.Add(-1);
            array.Add(right);
            return array;
        }


        public static List<int> Match(this List<int> array, int index, int left, int match)
        {
            array.Add(index);
            array.Add(0);
            array.Add(left);
            array.Add(match);
            array.Add(-1);
            return array;
        }

        public static List<int> Both(this List<int> array, int index, int left, int match, int right) =>
            array.Both(index, 0, left, match, right);

        public static List<int> Both(this List<int> array, int index, int state, int left, int match, int right)
        {
            array.Add(index);
            array.Add(state);
            array.Add(left);
            array.Add(match);
            array.Add(right);
            return array;
        }

    }
}
