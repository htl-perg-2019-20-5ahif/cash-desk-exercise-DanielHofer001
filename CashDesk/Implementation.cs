using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CashDesk
{
    /// <inheritdoc />
    public class DataAccess : IDataAccess
    {
        private CashDeskDbContext dataContext;

        private void CheckIfInitialized()
        {
            if (dataContext == null) throw new InvalidOperationException();

        }

        /// <inheritdoc />
        public Task InitializeDatabaseAsync()
        {
            if (dataContext == null)
            {
                dataContext = new CashDeskDbContext();
                return Task.CompletedTask;
            }

            throw new InvalidOperationException();
        }
        /// <inheritdoc />
        public async Task<int> AddMemberAsync(string firstName, string lastName, DateTime birthday)
        {
            CheckIfInitialized();
            if (string.IsNullOrEmpty(firstName)) {

                throw new ArgumentException("firstName is null");
            }
            if (string.IsNullOrEmpty(lastName))
            {
                throw new ArgumentException("lastName is null");
            }
            if (await dataContext.Members.AnyAsync((m => m.LastName == lastName)))
            {
                throw new DuplicateNameException();
            }
            Member add = new Member
            {
                FirstName = firstName,
                LastName = lastName,
                Birthday = birthday
            };
            await dataContext.AddAsync(add);
            await dataContext.SaveChangesAsync();
            return add.MemberNumber;
        }

       
        public async Task DeleteMemberAsync(int memberNumber)
        {
            CheckIfInitialized();
            Member remove = await dataContext.Members.FindAsync(memberNumber);
            if (remove == null) throw new ArgumentException("unknown memberNumber");
            dataContext.Members.Remove(remove);
            await dataContext.SaveChangesAsync();

        }

     
        public async Task<IMembership> JoinMemberAsync(int memberNumber)
        {
            CheckIfInitialized();
            Member m = await dataContext.Members.FindAsync(memberNumber);

            if (await dataContext.Memberships.AnyAsync((elem => elem.Member == m && elem.End == DateTime.MaxValue)))
            {
                throw new AlreadyMemberException();
            }
            Membership add = new Membership
            {
                Member = m,
                Begin = DateTime.Now,
                End = DateTime.MaxValue
            };
            dataContext.Memberships.Add(add);
            await dataContext.SaveChangesAsync();
            return add;
        }
      
        /// <inheritdoc />
        public async Task<IMembership> CancelMembershipAsync(int memberNumber)
        {
            CheckIfInitialized();
            Member m = await dataContext.Members.FindAsync(memberNumber);
           // if (m == null) throw new ArgumentException("unknown memberNumber");

            Membership ms= await dataContext.Memberships
                .FirstOrDefaultAsync((elem => elem.Member == m && elem.End >= DateTime.Now));
            if (ms==null)
            {
                throw new NoMemberException();
            }

            ms.End = DateTime.Now;
            dataContext.Memberships.Update(ms);
            await dataContext.SaveChangesAsync();
            return ms;
        }

        /// <summary>
        /// Deposit the specified amount for the specified member
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// <see cref="InitializeDatabaseAsync"/> has not been called before
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Unknown <paramref name="memberNumber"/> or invalid value in <paramref name="amount"/>.
        /// </exception>
        /// <exception cref="NoMemberException">
        /// The member is currently not an active member.
        /// </exception>

        /// <inheritdoc />
        public async Task DepositAsync(int memberNumber, decimal amount)
        {
            CheckIfInitialized();
            Member m = await dataContext.Members.FindAsync(memberNumber);

            try
            {
                Membership ms = await dataContext.Memberships
                    .FirstAsync((elem => elem.Member == m && elem.End >= DateTime.Now));
                Deposit add = new Deposit
                {
                    Membership = ms,
                    Amount = amount
                };
                dataContext.Deposits.Add(add);
                await dataContext.SaveChangesAsync();
            }
            catch (InvalidOperationException)
            {
                throw new NoMemberException();
            }

        }
        /// <summary>
        /// Gets statistics about deposits per member.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// <see cref="InitializeDatabaseAsync"/> has not been called before
        /// </exception>
        /// <inheritdoc />
        public async Task<IEnumerable<IDepositStatistics>> GetDepositStatisticsAsync()
        {
            CheckIfInitialized();
            return (await dataContext.Deposits.Include("Membership.Member").ToArrayAsync())
             .GroupBy(d => new { d.Membership.Begin.Year, d.Membership.Member })
             .Select(i => new DepositStatistics
             {
                 Year = i.Key.Year,
                 Member = i.Key.Member,
                 TotalAmount = i.Sum(d => d.Amount)
             });
            //Works also, but much more inefficient
            /*
             * var Stat = new List<IDepositStatistics>();
              foreach (var m in dataContext.Members)
               {
                   decimal sum = 0;
                   decimal year = 0;
                   if (m.Memberships != null)
                   {

                       year = m.Memberships.First().Begin.Year;
                       foreach (var s in m.Memberships)
                       { 
                           if (year.Equals(s.Begin.Year) && m.Memberships.IndexOf(s) != m.Memberships.Count-1)
                           {
                               sum += s.Deposits.Sum((s => s.Amount))
                           }
                           else
                           {
                               if (sum == 0) sum = s.Deposits.Sum((s => s.Amount));
                               DepositStatistics ds = new DepositStatistics
                               {
                                   Year = s.Begin.Year,
                                   Member = m,
                                   TotalAmount = sum
                               };
                               Stat.Add(ds);
                               sum = s.Deposits.Sum((s => s.Amount));


                           }
                           year = s.Begin.Year;
                       }
                   }
               }
               return Stat;*/
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (dataContext != null)
            {
                dataContext.Dispose();
                dataContext = null;
            }
        }
    }
}
