/// <summary>
/// 
/// </summary>
/// <filename></filename>
/// <version>1.0.0.0</version>
/// <modifications>
/// 	1.  ver 1.0.0.0     2006-00-00 00:00:00     Jongseok Song   -
///         : Created.
/// </modifications>
/// <copyright>Copyright (c) 2005~2007. EzControl, LG CNS All rights reserved.</copyright>
/// 

using System;
using System.Collections.Generic;
using System.Reflection;

namespace LGCNS.ezControl.Common
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SortableList<T> : List<T>
    {
        /// <summary>
        /// Sort with a comparer for objects of arbitrary types having using the specified properties
        /// </summary>
        /// <param name="strFields">Properties to sort objects by</param>
        public void Sort(params string[] strFields)
        {
            this.Sort(new ObjectComparer<T>(strFields));
        }
        /// <summary>
        /// Sort with a comparer for objects of arbitrary types having using the specified properties
        /// </summary>
        /// <param name="strFields">Properties to sort objects by</param>
        /// <param name="bStringToInt">�񱳽� string �� ���� Ÿ���̸� Int ���·� Sort</param>
        public void Sort(string[] strFields, bool bStringToInt)
        {
            this.Sort(new ObjectComparer<T>(strFields, bStringToInt));
        }
        /// <summary>
        ///Sort with a comparer for objects of arbitrary types having using the specified properties and sort order
        /// </summary>
        /// <param name="strFields">Properties to sort objects by</param>
        /// <param name="bIsDescending">Properties to sort in descending order</param>
        public int Sort(string[] strFields, bool[] bIsDescending)
        {
            if (strFields.Length != bIsDescending.Length) return -1;

            this.Sort(new ObjectComparer<T>(strFields, bIsDescending));

            return 0;
        }

        /// <summary>
        /// Sort with a comparer for objects of arbitrary types having using the specified properties and sort order
        /// </summary>
        /// <param name="strFields">Properties to sort objects by</param>
        /// <param name="bIsDescending">Properties to sort in descending order</param>
        /// <param name="bStringToInt">�񱳽� string �� ���� Ÿ���̸� Int ���·� Sort</param>
        /// <returns></returns>
        public int Sort(string[] strFields, bool[] bIsDescending, bool bStringToInt)
        {
            if (strFields.Length != bIsDescending.Length) return -1;

            this.Sort(new ObjectComparer<T>(strFields, bIsDescending, bStringToInt));

            return 0;
        }
        /// <summary>
        /// Find first one object that is matched with values using the specified properties 
        /// </summary>
        /// <param name="strFields">Properties to find first object by</param>
        /// <param name="values">Properties to compared by</param>
        /// <returns>found object</returns>
        public T Find(string[] strFields, object[] values)
        {
            if (strFields.Length != values.Length) return default(T);

            _strFieldList = strFields;
            _valueList = values;

            return this.Find(new Predicate<T>(Filter<T>));
        }

        /// <summary>
        /// Find all objects that is matched with values using the specified properties 
        /// </summary>
        /// <param name="strFields">Properties to find all objects by</param>
        /// <param name="values">Properties to compared by</param>
        /// <returns>found objects</returns>
        public SortableList<T> FindAll(string[] strFields, object[] values)
        {
            if (strFields.Length != values.Length) return null;

            _strFieldList = strFields;
            _valueList = values;

            List<T> list = this.FindAll(new Predicate<T>(Filter<T>));
            SortableList<T> listResult = new SortableList<T>();
            for (int i = 0; i < list.Count; i++)
            {
                listResult.Add(list[i]);
            }
            return listResult;
        }

        /// <summary>
        /// �⺻�� �ش� Feild �� ���� Value �� ������ �͸� ã��.
        /// bNot �� true �� ���� �ش� Field �� ���� value �� �ٸ� ���� ��� ã��.
        /// </summary>
        /// <param name="strFields"></param>
        /// <param name="values"></param>
        /// <param name="bNot"></param>
        /// <returns></returns>
        public SortableList<T> FindAll(string[] strFields, object[] values, bool bNot)
        {
            if (strFields.Length != values.Length) return null;

            _strFieldList = strFields;
            _valueList = values;

            List<T> list = null;

            if (bNot)
            {
                list = this.FindAll(new Predicate<T>(FilterNot<T>));
            }
            else
            {
                list = this.FindAll(new Predicate<T>(Filter<T>));
            }

            SortableList<T> listResult = new SortableList<T>();
            for (int i = 0; i < list.Count; i++)
            {
                listResult.Add(list[i]);
            }
            return listResult;
        }

        /// <summary>
        /// Field == value1 or value2 or ....value n  �ΰ� ã��.
        /// </summary>
        /// <param name="strField"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public SortableList<T> FindAll(string strField, object[] values)
        {
            //if (strFields.Length != values.Length) return null;

            _strField = strField;
            _valueList = values;

            List<T> list = this.FindAll(new Predicate<T>(FilterOR<T>));
            SortableList<T> listResult = new SortableList<T>();
            for (int i = 0; i < list.Count; i++)
            {
                listResult.Add(list[i]);
            }
            return listResult;
        }
        /// <summary>
        /// Field != value1 AND value2 AND ....value n  �ΰ� ã��.
        /// </summary>
        /// <param name="strField"></param>
        /// <param name="values"></param>
        /// <param name="bNot"></param>
        /// <returns></returns>
        public SortableList<T> FindAll(string strField, object[] values, bool bNot)
        {
            //if (strFields.Length != values.Length) return null;

            _strField = strField;
            _valueList = values;

            List<T> list = null;

            if (bNot)
            {
                list = this.FindAll(new Predicate<T>(FilterORNot<T>));
            }
            else
            {
                list = this.FindAll(new Predicate<T>(FilterOR<T>));
            }

            SortableList<T> listResult = new SortableList<T>();
            for (int i = 0; i < list.Count; i++)
            {
                listResult.Add(list[i]);
            }
            return listResult;
        }
        private string[] _strFieldList;
        private string   _strField;
        private object[] _valueList;
        private bool Filter<K>(K x)
        {
            //Get types of the objects
            Type typex = x.GetType();

            PropertyInfo pix = null;
            IComparable pvalx = null;

            for (int i = 0; i < _strFieldList.Length; i++)
            {
                //Get each property by name
                pix = typex.GetProperty(_strFieldList[i]);
                if (pix == null) return false;

                //Get the value of the property for each object
                pvalx = (IComparable)pix.GetValue(x, null);
                if (pvalx == null) return false;

                if (pvalx.CompareTo(_valueList[i]) != 0)
                {
                    return false;
                }
            }

            return true;
        }
        private bool FilterNot<K>(K x)
        {
            //Get types of the objects
            Type typex = x.GetType();

            PropertyInfo pix = null;
            IComparable pvalx = null;

            for (int i = 0; i < _strFieldList.Length; i++)
            {
                //Get each property by name
                pix = typex.GetProperty(_strFieldList[i]);
                if (pix == null) return false;

                //Get the value of the property for each object
                pvalx = (IComparable)pix.GetValue(x, null);
                if (pvalx == null) return false;

                if (pvalx.CompareTo(_valueList[i]) != 0)
                {
                    return true;
                }
            }

            return false;
        }
        private bool FilterOR<K>(K x)
        {
            //Get types of the objects
            Type typex = x.GetType();

            PropertyInfo pix = null;
            IComparable pvalx = null;

            //Get one property by name
            pix = typex.GetProperty(_strField);
            if (pix == null) return false;

            //Get the value of the property for one object
            pvalx = (IComparable)pix.GetValue(x, null);
            if (pvalx == null) return false;

            for (int i = 0; i < _valueList.Length; i++)
            {
                if (pvalx.CompareTo(_valueList[i]) == 0)
                {
                    return true;
                }
            }

            return false;
        }
        private bool FilterORNot<K>(K x)
        {
            //Get types of the objects
            Type typex = x.GetType();

            PropertyInfo pix = null;
            IComparable pvalx = null;

            //Get one property by name
            pix = typex.GetProperty(_strField);
            if (pix == null) return false;

            //Get the value of the property for one object
            pvalx = (IComparable)pix.GetValue(x, null);
            if (pvalx == null) return false;

            for (int i = 0; i < _valueList.Length; i++)
            {
                if (pvalx.CompareTo(_valueList[i]) != 0)
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectComparer<T> : IComparer<T>
    {
        #region protected fields
        /// <summary>
        /// Properties to sort objects by
        /// </summary>
        protected string[] _strFieldList;

        /// <summary>
        /// Properties to sort in descending order
        /// </summary>
        protected bool[] _bIsDescendingList;

        protected bool _bStringToInt = false;
        #endregion

        #region methods
        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to or greater than the other.
        /// </summary>
        /// <param name="x">First object to compare.</param>
        /// <param name="y">Second object to compare.</param>
        /// <returns></returns>
        public int Compare(T x, T y)
        {
            //Get types of the objects
            Type typex = x.GetType();
            Type typey = y.GetType();

            PropertyInfo pix = null;
            PropertyInfo piy = null;
            IComparable pvalx = null;
            object pvaly = null;

            for (int i = 0; i < _strFieldList.Length; i++)
            {
                //Get each property by name
                pix = typex.GetProperty(_strFieldList[i]);
                piy = typey.GetProperty(_strFieldList[i]);
                if (pix == null || piy == null)
                {
                    continue;
                }
                
                //Get the value of the property for each object
                pvalx = (IComparable)pix.GetValue(x, null);
                pvaly = piy.GetValue(y, null);

                int iResult;
                if (pvalx == null && pvaly == null)
                {
                    //nulls are equal
                    iResult = 0;
                }
                else if (pvalx == null && pvaly != null)
                {
                    //nulls is always less than anything else
                    iResult = -1;
                }
                else
                {
                    if (_bStringToInt)
                    {
                        // string ���¸� int �� �ٲپ� compare
                        int iValx = 0;
                        int iValy = 0;
                        if (int.TryParse(pvalx.ToString(), out iValx) && int.TryParse(pvaly.ToString(), out iValy))
                        {
                            iResult = iValx.CompareTo(iValy);
                        }
                        else
                        {
                             iResult = pvalx.CompareTo(pvaly);
                        }
                    }
                    else
                    {
                        //Compare values, using IComparable interface of the property's type
                        iResult = pvalx.CompareTo(pvaly);
                    }
                }

                if (iResult != 0)
                {
                    //Return if not equal
                    if (_bIsDescendingList[i])
                    {
                        //Invert order
                        return -iResult;
                    }
                    else
                    {
                        return iResult;
                    }
                }
            }

            return 0;
        }
        #endregion

        #region constructors
        /// <summary>
        /// Create a comparer for objects of arbitrary types having using the specified properties
        /// </summary>
        /// <param name="fields">Properties to sort objects by</param>
        public ObjectComparer(params string[] strFields)
            : this(strFields, new bool[strFields.Length])
        {
        }

        /// <summary>
        /// Create a comparer for objects of arbitrary types having using the specified properties
        /// </summary>
        /// <param name="fields">Properties to sort objects by</param>
        /// <param name="bStringToInt">�񱳽� string �� ���� Ÿ���̸� Int ���·� Sort</param>
        public ObjectComparer(string[] strFields, bool bStringToInt)
            : this(strFields, new bool[strFields.Length])
        {
            _bStringToInt = bStringToInt;
        }

        /// <summary>
        /// Create a comparer for objects of arbitrary types having using the specified properties and sort order
        /// </summary>
        /// <param name="strFields">Properties to sort objects by</param>
        /// <param name="bIsDescending">Properties to sort in descending order</param>
        public ObjectComparer(string[] strFields, bool[] bIsDescending)
        {
            _strFieldList = strFields;
            _bIsDescendingList = bIsDescending;
        }
        /// <summary>
        /// Create a comparer for objects of arbitrary types having using the specified properties and sort order
        /// </summary>
        /// <param name="strFields">Properties to sort objects by</param>
        /// <param name="bIsDescending">Properties to sort in descending order</param>
        /// /// <param name="bStringToInt">�񱳽� string �� ���� Ÿ���̸� Int ���·� Sort</param>
        public ObjectComparer(string[] strFields, bool[] bIsDescending, bool bStringToInt)
        {
            _strFieldList = strFields;
            _bIsDescendingList = bIsDescending;
            _bStringToInt = bStringToInt;
        }
        #endregion
    }
}
