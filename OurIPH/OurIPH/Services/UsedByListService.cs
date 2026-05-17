using System;
using System.Linq;

namespace OurIPH.Services
{
    public class UsedByListService
    {
        public string Add(string current, string productName)
        {
            if (string.IsNullOrWhiteSpace(productName))
            {
                return current ?? "";
            }

            if (string.IsNullOrWhiteSpace(current))
            {
                return productName;
            }

            var parts = current.Split(new[] { ", " }, StringSplitOptions.None);
            return parts.Contains(productName) ? current : current + ", " + productName;
        }
    }
}
