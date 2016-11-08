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
    public class CheckHandler<T>
    {
        /// <summary>
        /// The checks
        /// </summary>
        private List<Func<T, Boolean>> checks { get; set; }

        /// <summary>
        /// Creates a new instance
        /// </summary>
        public CheckHandler()
        {
            checks = new List<Func<T, Boolean>>();
        }

        /// <summary>
        /// Adds a check to the checker
        /// </summary>
        public void Add(Func<T, Boolean> check)
        {
            checks.Add(check);
        }

        /// <summary>
        /// Removes a check from the checker
        /// </summary>
        public void Remove(Func<T, Boolean> check)
        {
            checks.Remove(check);
        }

        /// <summary>
        /// Returns all checks, combined with &&
        /// </summary>
        public Boolean And(T value)
        {
            if (!checks.Any())
                return true;
            Boolean result = checks[0](value);
            for (Int32 i = 1; i < checks.Count; i++)
            {
                result &= checks[i](value);
            }
            return result;
        }

        /// <summary>
        /// Returns all checks, combined with ||
        /// </summary>
        public Boolean Or(T value)
        {
            if (!checks.Any())
                return true;
            Boolean result = checks[0](value);
            for (Int32 i = 1; i < checks.Count; i++)
            {
                result |= checks[i](value);
            }
            return result;
        }
    }
}