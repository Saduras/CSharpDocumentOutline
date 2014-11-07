using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DavidSpeck.CSharpDocOutline.CDM
{
    // CodeElementAccessModifier
    public enum CEAccessModifier
    {
        None,
        Public,
        Protected,
        Private,
        Internal,
        Protected_Internal
    }

    public static class CEAccessModifierHelper
    {
		/// <summary>
		/// Helper to parse a CEAccessModifier from a given string.
		/// </summary>
        public static CEAccessModifier Parse(string str, CEAccessModifier defaultValue)
        {
            str.Trim();

            // Enable parsing of "procteced internal" by replacing the space 
            str.Replace(" ", "_");

            CEAccessModifier result = CEAccessModifier.None;
            Enum.TryParse<CEAccessModifier>(str, true, out result);

            if (result == CEAccessModifier.None)
                result = defaultValue;

            return result;
        }
    }
}