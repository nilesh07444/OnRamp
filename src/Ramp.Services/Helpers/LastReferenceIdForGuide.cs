using System;
using System.Globalization;

namespace Ramp.Services.Helpers
{
    public static class LastReferenceIdForGuide
    {
        public static string GetGuideReference(string referId)
        {
            #region UtilityforRF000001REFId

            // code to generate the RefId Automatically at a time of Create

            string referenceId;

            string[] refId = referId.Split('F');
            if (string.IsNullOrEmpty(referId))
            {
                referenceId = "RF000001";
            }
            else
            {
                int id = Convert.ToInt32(refId[1]);
                int idRef = id + 1;
                string refIdString = "RF";
                if (idRef.ToString(CultureInfo.InvariantCulture).Length != 6)
                {
                    for (int i = 0; i < (6 - idRef.ToString(CultureInfo.InvariantCulture).Length); i++)
                    {
                        refIdString += "0";
                    }
                }
                refIdString += idRef.ToString(CultureInfo.InvariantCulture);
                referenceId = refIdString;
            }
            return referenceId;

            #endregion UtilityforRF000001REFId
        }
    }
}