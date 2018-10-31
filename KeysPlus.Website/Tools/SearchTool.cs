namespace KeysPlus.Website.Areas.Tools
{
    public class SearchTool
    {
        //public string FormatString { get; set; }

        public int CheckDisplayType(string searchString)
        {
            //trim redundant space
            string newString = searchString.Trim();
            //check whether search string starts from *,e,g. **key
            //trim all * and display results ending with key
            if (newString[0] == '*')
            {
                return 1; // 1 means ends with
            }
            //check whether search string ends at *,e,g. key**
            //trim all * and display results starts with key
            else if (newString[newString.Length - 1] == '*') //Bug Fix #2082 : Out of bound array error
            {
                return 2; // 2 means starts with
            }
            else
            {
                return 3; //3 means contains
            }
        }

        public string ConvertString(string searchString)
        {
            //convert search string to lower case and trim redundant space
            searchString = searchString.ToLower().Trim();
            string formatString = searchString;
            int type = CheckDisplayType(searchString);
            //type 1 or 2 means there is(are) * in the input string
            //trim all the * in the string
            if (type <= 2)
            {
                formatString = searchString.Replace('*', ' ').Trim();
               
            }
           
            return formatString;
        }
    }
}