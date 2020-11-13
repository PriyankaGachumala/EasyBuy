using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyBuy.Models.ViewModels
{
    public class MainPageViewModel
    {
        public IEnumerable<Category> Category { get; set; }
        public IEnumerable<Product> Product { get; set; }
        public IEnumerable<Coupon> Coupon { get; set; }
    }
}
