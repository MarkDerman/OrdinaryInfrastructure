

using Odin.DesignContracts;

namespace Targets
{
    public sealed class PostconditionsTestTarget
    {
        public int Number { get; set; } 
        public string String { get; set; } = "";
        
        /// <summary>
        /// number = -1 -> failure
        /// number = 1 -> fine
        /// </summary>
        /// <param name="number"></param>
        public void VoidSingleConditionSingleReturn(int number)
        {
            Contract.Ensures(Number > 0);
            Number = number;
        }
        
        /// <summary>
        /// 2
        /// </summary>
        /// <param name="number"></param>
        public void VoidSingleConditionMultipleReturn(int number)
        {
            Contract.Ensures(Number > 0);
            if (number > 50)
            {
                Number = -100;
                return;
            }
            Number = number;
        }
        
        /// <summary>
        /// 3
        /// </summary>
        /// <param name="number"></param>
        /// <param name="aString"></param>
        public void VoidMultipleConditionsSingleReturn(int number, string aString)
        {
            Contract.Ensures(Number > 0);
            Contract.Ensures(String.Length == 2);
            Contract.Ensures(Number > 0);
            Number = number;
            String = aString;
        }
        
        /// <summary>
        /// 4
        /// </summary>
        /// <param name="number"></param>
        /// <param name="aString"></param>
        public void VoidMultipleConditionsMultipleReturn(int number, string aString)
        {
            Contract.Ensures(Number > 0);
            Contract.Ensures(String.Length == 2);
            String = aString;
            if (number > 100)
            {
                Number = -100;
                return;
            }
            Number = number;
        }
        
        /// <summary>
        /// 5
        /// </summary>
        /// <param name="number"></param>
        public int NotVoidSingleConditionSingleReturn(int number)
        {
            Contract.Ensures(Number > 0);
            Number = number;
            return Number;
        }
        
        /// <summary>
        /// 6
        /// </summary>
        /// <param name="number"></param>
        public int NotVoidSingleConditionMultipleReturn(int number)
        {
            Contract.Ensures(Number > 0);
            if (number > 100)
            {
                Number = -100;
                return Number;
            }
            Number = number;
            return Number;
        }
      
        /// <summary>
        /// 7
        /// </summary>
        /// <param name="number"></param>
        /// <param name="aString"></param>
        public void NotVoidMultipleConditionsSingleReturn(int number, string aString)
        {
            Contract.Ensures(Number > 0);
            Contract.Ensures(String.Length == 2);
            Contract.Ensures(Number > 0);
            Number = number;
            String = aString;
        }
        
        /// <summary>
        /// 8
        /// </summary>
        /// <param name="number"></param>
        /// <param name="aString"></param>
        public void NotVoidMultipleConditionsMultipleReturn(int number, string aString)
        {
            Contract.Ensures(Number > 0);
            Contract.Ensures(String.Length == 2);
            String = aString;
            if (number > 100)
            {
                Number = -100;
                return;
            }
            Number = number;
        }
    }
}
