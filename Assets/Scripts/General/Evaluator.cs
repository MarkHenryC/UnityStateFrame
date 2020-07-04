using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QuiteSensible
{
    public class Evaluator
    {
        public enum Mode { Int, Bool, String };

        private FuncTableInt funcTableInt = new FuncTableInt();
        private FuncTableBool funcTableBool = new FuncTableBool();
        private FuncTableString funcTableString = new FuncTableString();
        private Action fallback;        
        private Mode mode;        

        /// <summary>
        /// Check if the first n items (the 
        /// length of input) are equal to the
        /// correct answers. Actually if 
        /// input is longer than answers then
        /// it should be an error.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="correct"></param>
        /// <returns></returns>
        public static bool QuickStringEval(LinkedList<string> input, params string[] correct)
        {
            if (input.Count != correct.Length)
                return false;

            var n = input.GetEnumerator();
                
            foreach (var c in correct)
            {
                if (n.MoveNext())
                {
                    if (c != null && c != n.Current) // null in correct array means compare doesn't matter
                        return false;
                }
                else
                    return false; // input too short
            }
            return true;
        }

        public Evaluator(Mode m)
        {
            mode = m;
        }

        public void SetFallback(Action a)
        {
            switch (mode)
            {
                case Mode.Bool:
                    funcTableBool.DefaultAction = a;
                    break;
                case Mode.Int:
                    funcTableInt.DefaultAction = a;
                    break;
                case Mode.String:
                    funcTableString.DefaultAction = a;
                    break;
            }
        }

        public void AddCondition(Action a, params string[] sequence)
        {
            funcTableString[new ConditionString(sequence)] = a;
        }

        public void AddCondition(Action a, params int?[] sequence)
        {
            funcTableInt[new ConditionInt(sequence)] = a;
        }

        public void AddCondition(Action a, params bool?[] sequence)
        {
            funcTableBool[new ConditionBool(sequence)] = a;
        }

        public Action this[params string[] sequence]
        {
            get
            {
                return funcTableString[new ConditionString(sequence)];
            }
        }

        public Action this[params int?[] sequence]
        {
            get
            {
                return funcTableInt[new ConditionInt(sequence)];
            }
        }

        public Action this[params bool?[] sequence]
        {
            get
            {
                return funcTableBool[new ConditionBool(sequence)];
            }
        }


        public void TestBool()
        {
            var c1 = new ConditionBool(true, true, false);
            var c2 = new ConditionBool(false, true, true);
            var c3 = new ConditionBool(true, true, true);
            var c4 = new ConditionBool(null, false, null, true);

            var funcTable = new FuncTableBool();

            funcTable[c1] = delegate () { Debug.Log(c1); };
            funcTable[c2] = delegate () { Debug.Log(c2); };
            funcTable[c3] = delegate () { Debug.Log(c3); };
            funcTable[c4] = delegate () { Debug.Log(c4); };

            Debug.Log("Test exact objects ---- >");

            funcTable[c1]();
            funcTable[c2]();
            funcTable[c3]();
            funcTable[c4]();

            Debug.Log("Test ephemeral indices ---- >");

            funcTable[new ConditionBool(true, true, false)]();
            funcTable[new ConditionBool(false, true, true)]();
            funcTable[new ConditionBool(true, true, true)]();
            funcTable[new ConditionBool(null, false, null, true)]();
        }

        public void TestInt()
        {
            var c1 = new ConditionInt(1, 2, 3, 4);
            var c2 = new ConditionInt(3, 4, 5, 6, 7, 8);
            var c3 = new ConditionInt(2, 4);
            var c4 = new ConditionInt(1);

            var funcTable = new FuncTableInt();

            funcTable[c1] = delegate () { Debug.Log(c1); };
            funcTable[c2] = delegate () { Debug.Log(c2); };
            funcTable[c3] = delegate () { Debug.Log(c3); };
            funcTable[c4] = delegate () { Debug.Log(c4); };

            Debug.Log("Test exact objects ---- >");

            funcTable[c1]();
            funcTable[c2]();
            funcTable[c3]();
            funcTable[c4]();

            Debug.Log("Test ephemeral indices ---- >");

            funcTable[new ConditionInt(1, 2, 3, 4)]();
            funcTable[new ConditionInt(3, 4, 5, 6, 7, 8)]();
            funcTable[new ConditionInt(2, 4)]();
            funcTable[new ConditionInt(1)]();
        }
    }

    public class FuncTable
    {
        public Action DefaultAction { set; get; }
    }

    public class FuncTableBool : FuncTable
    {
        private Dictionary<ConditionBool, Action> conditionDict = new Dictionary<ConditionBool, Action>();

        public Action this[ConditionBool c]
        {
            get
            {
                Action val;
                if (!conditionDict.TryGetValue(c, out val))
                {
                    if (DefaultAction != null)
                        return DefaultAction;
                    else
                        return delegate () { Debug.Log("Unimplemented combination: " + c); };
                }
                return conditionDict[c];
            }
            set
            {
                conditionDict[c] = value;
            }
        }
    }

    public class FuncTableInt : FuncTable
    {
        private Dictionary<ConditionInt, Action> conditionDict = new Dictionary<ConditionInt, Action>();
        
        public Action this[ConditionInt c]
        {
            get
            {
                Action val;
                if (!conditionDict.TryGetValue(c, out val))
                {
                    if (DefaultAction != null)
                        return DefaultAction;
                    else
                        return delegate () { Debug.Log("Unimplemented combination: " + c); };
                }
                else
                    return conditionDict[c];
            }
            set
            {
                conditionDict[c] = value;
            }
        }
    }

    public class FuncTableString : FuncTable
    {
        private Dictionary<ConditionString, Action> conditionDict = new Dictionary<ConditionString, Action>();

        public Action this[ConditionString c]
        {
            get
            {
                Action val;
                if (!conditionDict.TryGetValue(c, out val))
                {
                    if (DefaultAction != null)
                        return DefaultAction;
                    else
                        return delegate () { Debug.Log("Unimplemented combination: " + c); };
                }
                return conditionDict[c];
            }
            set
            {
                conditionDict[c] = value;
            }
        }
    }

    public class ConditionBool : EqualityComparer<ConditionBool>
    {
        private List<bool?> flags;

        private int HashField(int h, bool? cond)
        {
            return h * 23 + cond.GetHashCode();
        }

        /// <summary>
        /// If one side has a null we don't care
        /// about comparing it
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private static bool Equality(ConditionBool x, ConditionBool y)
        {
            if (x.flags.Count != y.flags.Count)
                return false;
            for (int i = 0; i < x.flags.Count; i++)
            {
                if (x.flags[i].HasValue && y.flags[i].HasValue)
                {
                    if (x.flags[i] != y.flags[i])
                        return false;
                }
            }
            return true;
        }

        private int Hash()
        {
            unchecked
            {
                int hash = 17;

                for (int i = 0; i < flags.Count; i++)
                    hash = HashField(hash, flags[i]);

                return hash;
            }
        }

        public ConditionBool(params bool?[] _flags)
        {
            flags = new List<bool?>();
            flags.AddRange(_flags);
        }

        public override string ToString()
        {
            string s = "Flags: ";
            foreach (var f in flags)
                s += (f.HasValue ? f.Value.ToString() : "null") + " ";
            return s;
        }

        public override int GetHashCode(ConditionBool obj)
        {
            return Hash();
        }

        public override int GetHashCode()
        {
            return Hash();
        }

        public override bool Equals(ConditionBool x, ConditionBool y)
        {
            return Equality(x, y);
        }

        public override bool Equals(object obj)
        {
            return Equality(this, obj as ConditionBool);
        }

        public static bool operator ==(ConditionBool x, ConditionBool y)
        {
            return Equality(x, y);
        }

        public static bool operator !=(ConditionBool x, ConditionBool y)
        {
            return !Equality(x, y);
        }
    }

    public class ConditionInt : EqualityComparer<ConditionInt>
    {
        private List<int?> flags;

        private int HashField(int h, int? cond)
        {
            return h * 23 + cond.GetHashCode();
        }

        /// <summary>
        /// If one side has a null we don't care
        /// about comparing it
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private static bool Equality(ConditionInt x, ConditionInt y)
        {
            if (x.flags.Count != y.flags.Count)
                return false;
            for (int i = 0; i < x.flags.Count; i++)
            {
                if (x.flags[i].HasValue && y.flags[i].HasValue)
                {
                    if (x.flags[i] != y.flags[i])
                        return false;
                }
            }
            return true;
        }

        private int Hash()
        {
            unchecked
            {
                int hash = 17;

                for (int i = 0; i < flags.Count; i++)
                    hash = HashField(hash, flags[i]);

                return hash;
            }
        }

        public ConditionInt(params int?[] _flags)
        {
            flags = new List<int?>();
            flags.AddRange(_flags);
        }

        public override string ToString()
        {
            string s = "Flags: ";
            foreach (var f in flags)
                s += (f.HasValue ? f.Value.ToString() : "null") + " ";
            return s;
        }

        public override int GetHashCode(ConditionInt obj)
        {
            return Hash();
        }

        public override int GetHashCode()
        {
            return Hash();
        }

        public override bool Equals(ConditionInt x, ConditionInt y)
        {
            return Equality(x, y);
        }

        public override bool Equals(object obj)
        {
            return Equality(this, obj as ConditionInt);
        }

        public static bool operator ==(ConditionInt x, ConditionInt y)
        {
            return Equality(x, y);
        }

        public static bool operator !=(ConditionInt x, ConditionInt y)
        {
            return !Equality(x, y);
        }
    }

    public class ConditionString : EqualityComparer<ConditionString>
    {
        private List<string> flags;

        private int HashField(int h, string cond)
        {
            return h * 23 + cond.GetHashCode();
        }

        /// <summary>
        /// If one side has a null we don't care
        /// about comparing it
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private static bool Equality(ConditionString x, ConditionString y)
        {
            if (x.flags.Count != y.flags.Count)
                return false;
            for (int i = 0; i < x.flags.Count; i++)
            {
                if (!string.IsNullOrEmpty(x.flags[i]) && !string.IsNullOrEmpty(y.flags[i]))
                {
                    if (x.flags[i] != y.flags[i])
                        return false;
                }
            }
            return true;
        }

        private int Hash()
        {
            unchecked
            {
                int hash = 17;

                for (int i = 0; i < flags.Count; i++)
                    hash = HashField(hash, flags[i]);

                return hash;
            }
        }

        public ConditionString(params string[] _flags)
        {
            flags = new List<string>();
            flags.AddRange(_flags);
        }

        public override string ToString()
        {
            string s = "Flags: ";
            foreach (var f in flags)
                s += (!string.IsNullOrEmpty(f) ? f.ToString() : "null") + " ";
            return s;
        }

        public override int GetHashCode(ConditionString obj)
        {
            return Hash();
        }

        public override int GetHashCode()
        {
            return Hash();
        }

        public override bool Equals(ConditionString x, ConditionString y)
        {
            return Equality(x, y);
        }

        public override bool Equals(object obj)
        {
            return Equality(this, obj as ConditionString);
        }

        public static bool operator ==(ConditionString x, ConditionString y)
        {
            return Equality(x, y);
        }

        public static bool operator !=(ConditionString x, ConditionString y)
        {
            return !Equality(x, y);
        }
    }

}