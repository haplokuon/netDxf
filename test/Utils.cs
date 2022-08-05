namespace netDxf.Tests
{

    public static partial class Ext
    {

        public static void AssertEqualsTol(this double actual, double tol, double expected, string userMessage = "")
        {
            if (!MathHelper.IsEqual(expected, actual, tol))
                throw new Xunit.Sdk.AssertActualExpectedException(expected, actual, userMessage);
        }

        public static void AssertEqualsTol(this Vector3 actual, double tol,
            double expectedX, double expectedY, double expectedZ) =>
            actual.Equals(new Vector3(expectedX, expectedY, expectedZ), tol);

        public static void AssertEqualsTol(this Vector3 actual, double tol, Vector3 expected, string userMessage = "")
        {
            if (!expected.Equals(actual, tol))
                throw new Xunit.Sdk.AssertActualExpectedException(expected, actual, userMessage);
        }
        
    }

}