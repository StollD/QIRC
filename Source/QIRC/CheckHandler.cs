/** 
 * .NET Bot for Internet Relay Chat (IRC)
 * Copyright (c) ThomasKerman 2016
 * QIRC is licensed under the MIT License
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace QIRC
{
    /// <summary>
    /// Class that combines checks (i.e. methods that return a boolean)
    /// </summary>
    public class CheckHandler
    {
        /// <summary>
        /// The checks
        /// </summary>
        private List<Func<Boolean>> checks { get; set; }

        /// <summary>
        /// Creates a new instance
        /// </summary>
        public CheckHandler()
        {
            checks = new List<Func<Boolean>>();
        }

        /// <summary>
        /// Adds a check to the checker
        /// </summary>
        public void Add(Func<Boolean> check)
        {
            checks.Add(check);
        }

        /// <summary>
        /// Removes a check from the checker
        /// </summary>
        public void Remove(Func<Boolean> check)
        {
            checks.Remove(check);
        }

        /// <summary>
        /// Returns all checks, combined with &&
        /// </summary>
        public Boolean And()
        {
            if (!checks.Any())
                return true;
            Boolean result = checks[0]();
            for (Int32 i = 1; i < checks.Count; i++)
            {
                result &= checks[i]();
            }
            return result;
        }

        /// <summary>
        /// Returns all checks, combined with ||
        /// </summary>
        public Boolean Or()
        {
            if (!checks.Any())
                return true;
            Boolean result = checks[0]();
            for (Int32 i = 1; i < checks.Count; i++)
            {
                result |= checks[i]();
            }
            return result;
        }
    }
}