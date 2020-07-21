using System;
using System.Collections.Generic;
using System.Data;

namespace CoP.Enterprise
{
    [Serializable]
    public class BiDirectionalDictionary<L, R>
    {
        private readonly SerializableDictionary<L, R> leftToRight = new SerializableDictionary<L, R>();
        private readonly SerializableDictionary<R, L> rightToLeft = new SerializableDictionary<R, L>();
        public void Add(L leftSide, R rightSide)
        {
            if (leftToRight.ContainsKey(leftSide) ||
                rightToLeft.ContainsKey(rightSide))
                throw new DuplicateNameException();
            leftToRight.Add(leftSide, rightSide);
            rightToLeft.Add(rightSide, leftSide);
        }


        public int CountLeft => leftToRight.Count;
        public int CountRight => rightToLeft.Count;

        public bool ContainsLeftKey(L leftSideKey)
        { return leftToRight.ContainsKey(leftSideKey); }

        public bool ContainsRightKey(R rightSideKey)
        { return rightToLeft.ContainsKey(rightSideKey); }
    }
    public class DoubleDictionary<K1, K2, V>: Dictionary<K1, Dictionary<K2, V>> 
    { }
}