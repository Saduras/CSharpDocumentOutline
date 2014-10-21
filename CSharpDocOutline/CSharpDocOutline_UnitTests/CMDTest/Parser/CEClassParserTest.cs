using DavidSpeck.CSharpDocOutline.CDM;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpDocOutline_UnitTests.CMDTest.Parser
{
    [TestClass()]
    public class CEClassParserTest
    {
        [TestMethod()]
        public void TestParse()
        {
            // Test-Case: proper class definition
            var classString = "public class MyClass {";

            var parser = new CEClassParser();
            var result = parser.Parse(classString, 100);

            Assert.AreEqual(CEAccessModifier.Public, result.AccessModifier);
            Assert.AreEqual(100, result.LineNumber);
            Assert.AreEqual("MyClass", result.ElementName);
            Assert.AreEqual(CEKind.Class, result.Kind);

            // Test-Case: class without access modifier
            classString = "class MyClass {";

            parser = new CEClassParser();
            result = parser.Parse(classString, 100);

            Assert.AreEqual(CEAccessModifier.Internal, result.AccessModifier);
            Assert.AreEqual(100, result.LineNumber);
            Assert.AreEqual("MyClass", result.ElementName);
            Assert.AreEqual(CEKind.Class, result.Kind);

            // Test-Case: invalid statement
            classString = "var myVariable = 100f;";

            parser = new CEClassParser();
            result = parser.Parse(classString, 100);

            Assert.AreEqual(null, result);
        }

        [TestMethod()]
        public void TestSomehting()
        {
            string str = "  _{";
            str.Remove(1, 2);

            Assert.IsTrue(string.IsNullOrEmpty(str));
        }
    }
}
