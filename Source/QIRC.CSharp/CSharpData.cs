/** 
 * .NET Bot for Internet Relay Chat (IRC)
 * Copyright (c) Dorian Stoll 2017
 * QIRC is licensed under the MIT License
 */

using System;
using QIRC.Serialization;
using SQLite;

namespace QIRC.CSharp
{
    /// <summary>
    /// A C# Expression
    /// </summary>
    public class CSharpData : Storage<CSharpData>
    {
        [PrimaryKey, NotNull, AutoIncrement, Unique]
        public Int32 Index { get; set; }

        [NotNull]
        public String Expression { get; set; }

        public CSharpData() { }

        public CSharpData(String exp)
        {
            Expression = exp;
        }
    }
}