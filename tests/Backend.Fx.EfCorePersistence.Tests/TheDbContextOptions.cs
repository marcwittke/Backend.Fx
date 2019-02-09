//using System;
//using System.Data;
//using System.Data.Common;
//using System.Data.SqlClient;
//using System.Linq;
//using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Domain;
//using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Persistence;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Storage;
//using Xunit;

//namespace Backend.Fx.EfCorePersistence.Tests
//{
//    /// <summary>
//    /// Explorative test for SQL Server transaction handling, works only locally
//    /// </summary>
//    [Obsolete]
//    public class TheDbContextOptions : IDisposable
//    {
//        //[Fact]
//        public void WithConnectionStringResultsInMultipleConnections()
//        {
//            var options = new DbContextOptionsBuilder<TestDbContext>().UseSqlServer(ConnectionString).Options;
//            using (var dbContext = new TestDbContext(options))
//            {
//                // doing one command opens and closes the connection and transaction
//                dbContext.Bloggers.Add(new Blogger(1, "Skywalker", "Luke"));
//                dbContext.SaveChanges();

//                // result is visible
//                var blogger = dbContext.Bloggers.FromSql("SELECT * FROM Bloggers WHERE Id = 1").FirstOrDefault();
//                Assert.NotNull(blogger);

//                // even in another dbContext
//                using (var dbContext2 = new TestDbContext(options))
//                {
//                    var blogger2 = dbContext2.Bloggers.FromSql("SELECT * FROM Bloggers WHERE Id = 1").FirstOrDefault();
//                    Assert.NotNull(blogger2);
//                }

//                // therefore this connection instance is closed and has to be opened before
//                // using it. It also does not share the same transaction!
//                var conn = dbContext.Database.GetDbConnection();
//                Assert.Equal(ConnectionState.Closed, conn.State);
//            }
//        }


//        //[Fact]
//        public void WithConnectionResultsInOneConnection()
//        {
//            using (var connection = new SqlConnection(ConnectionString))
//            {
//                connection.Open(); // this is mandatory! As well as closing!
//                using (DbTransaction tx = connection.BeginTransaction())
//                {
//                    var options = new DbContextOptionsBuilder<TestDbContext>().UseSqlServer(connection).Options;

//                    using (var dbContext = new TestDbContext(options))
//                    {
//                        dbContext.Database.UseTransaction(tx);
//                        dbContext.Bloggers.Add(new Blogger(1, "Skywalker", "Luke"));
//                        dbContext.SaveChanges();

//                        Assert.Equal(tx, dbContext.Database.CurrentTransaction.GetDbTransaction());

//                        // hand crafted command
//                        var conn = dbContext.Database.GetDbConnection();
//                        Assert.Equal(ConnectionState.Open, conn.State);
//                        var cmd = conn.CreateCommand();
//                        cmd.Transaction = tx;
//                        cmd.CommandText = "SELECT LastName FROM Bloggers WHERE Id = 1";
//                        string lastName = (string)cmd.ExecuteScalar();
//                        Assert.Equal("Skywalker", lastName);

//                        // command on DbSet<T>
//                        var blogger = dbContext.Bloggers.FromSql("SELECT * FROM Bloggers WHERE Id = 1").FirstOrDefault();
//                        Assert.NotNull(blogger);

//                        ////this would deadlock
//                        //using (var connection2 = new SqlConnection(ConnectionString))
//                        //{
//                        //    connection2.Open();
//                        //    using (DbTransaction tx2 = connection2.BeginTransaction())
//                        //    {
//                        //        var options2 = new DbContextOptionsBuilder<TestDbContext>().UseSqlServer(connection2).Options;
//                        //        using (var dbContext2 = new TestDbContext(options2))
//                        //        {
//                        //            dbContext2.Database.UseTransaction(tx2);
//                        //            Assert.Empty(dbContext2.Bloggers);
//                        //        }
//                        //    }
//                        //}
//                    }
//                }
//            }

//            // rolled back, so asserting for empty
//            using (var connection2 = new SqlConnection(ConnectionString))
//            {
//                connection2.Open();
//                using (DbTransaction tx2 = connection2.BeginTransaction())
//                {
//                    var options2 = new DbContextOptionsBuilder<TestDbContext>().UseSqlServer(connection2).Options;
//                    using (var dbContext2 = new TestDbContext(options2))
//                    {
//                        dbContext2.Database.UseTransaction(tx2);
//                        Assert.Empty(dbContext2.Bloggers);
//                    }
//                }
//            }

//            // or even without tx
//            using (var connection3 = new SqlConnection(ConnectionString))
//            {
//                connection3.Open();
//                var options2 = new DbContextOptionsBuilder<TestDbContext>().UseSqlServer(connection3).Options;
//                using (var dbContext2 = new TestDbContext(options2))
//                {
//                    Assert.Empty(dbContext2.Bloggers);
//                }
//            }
//        }

//        public TheDbContextOptions() : base("TheDbContextOptions")
//        {
//            using (var dbContext = UseDbContext())
//            {
//                dbContext.Database.EnsureCreated();
//            }
//        }

//    }
//}
