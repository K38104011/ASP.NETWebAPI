﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PingYourPackage.Domain.Entities
{
    public interface IEntityRepository<T> where T : class, IEntity, new()
    {
        IQueryable<T> AllIncluding(params Expression<Func<T, object>>[] includeProperties);
        IQueryable<T> All { get; }
        IQueryable<T> GetAll();
        T GetSingle(Guid key);
        IQueryable<T> FindBy(Expression<Func<T, bool>> predicate);

        void Add(T entity);
        void Delete(T entity);
        void Edit(T entity);
        void Save();
    }
}