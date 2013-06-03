using ZMQ.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ZMQ.Net.Test
{


    /// <summary>
    ///This is a test class for ContextTest and is intended
    ///to contain all ContextTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ContextTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for Context Constructor
        ///</summary>
        [TestMethod()]
        public void ContextConstructorTest()
        {
            Context ctx = new Context( 0 );
            Assert.IsNotNull( ctx );
            Assert.IsFalse( ctx.Disposed );
        }

        /// <summary>
        ///A test for Context Constructor
        ///</summary>
        [TestMethod()]
        public void ContextConstructorTest1()
        {
            Context ctx = new Context();
            Assert.IsNotNull( ctx );
            Assert.IsFalse( ctx.Disposed );
        }

        /// <summary>
        ///A test for CreateSocket
        ///</summary>
        [TestMethod()]
        public void CreateSocketTest()
        {
            Context ctx = new Context();
            Socket socket = ctx.CreateSocket( SocketType.Publisher );
            Assert.IsNotNull( socket );
        }

        /// <summary>
        ///A test for Dispose
        ///</summary>
        [TestMethod()]
        public void DisposeTest()
        {
            Context ctx = new Context();
            ctx.Dispose();
            Assert.IsTrue( ctx.Disposed );

            try
            {
                ctx.CreateSocket( SocketType.Publisher );
                Assert.Fail( "Disposed Context should not be able to create sockets." );
            }
            catch
            {
            }
        }
    }
}
