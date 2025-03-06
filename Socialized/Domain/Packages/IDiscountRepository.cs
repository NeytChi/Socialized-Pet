﻿namespace Domain.Packages
{
    public interface IDiscountRepository
    {
        DiscountPackage GetBy(int month);
        ICollection<DiscountPackage> GetAll();
    }
}
