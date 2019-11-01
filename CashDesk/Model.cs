using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CashDesk
{
    public class Member: IMember
    {
        [Key]
        public int MemberNumber { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public DateTime Birthday { get; set; }
        public List<Membership> Memberships { get; set; }
    }
    public class Membership : IMembership
    {

        public int MembershipId { get; set; }

        [Required]
        public Member Member { get; set; }

        private DateTime begin = DateTime.MinValue;

        public DateTime Begin { get { return begin; } set { begin = value; } }

        public DateTime End { get; set; }
        IMember IMembership.Member => Member;
        public List<Deposit> Deposits { get; set; }

    }
    public class Deposit : IDeposit
    {
        public int DepositId { get; set; }

        [Required]
        public Membership Membership { get; set; }

        [Required]
        [Range(0, float.MaxValue)]
        public decimal Amount { get; set; }
        IMembership IDeposit.Membership
        {
            get
            {
                return Membership;
            }
        }
    }
    public class DepositStatistics : IDepositStatistics
    {
        public IMember Member { get; set; }

        public int Year { get; set; }

        public decimal TotalAmount { get; set; }
    }

}
