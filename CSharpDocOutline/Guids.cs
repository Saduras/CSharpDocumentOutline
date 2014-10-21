// Guids.cs
// MUST match guids.h
using System;

namespace DavidSpeck.CSharpDocOutline
{
    static class GuidList
    {
        public const string guidCSharpDocOutlinePkgString = "07e67e82-fe60-4f9c-8056-c2194e2c45a2";
        public const string guidCSharpDocOutlineCmdSetString = "e544a7c7-d78e-4f60-83a9-3f0ea736a3dd";
        public const string guidToolWindowPersistanceString = "bc1bea3f-26f8-4e72-b3c6-087cd5b46332";

        public static readonly Guid guidCSharpDocOutlineCmdSet = new Guid(guidCSharpDocOutlineCmdSetString);
    };
}