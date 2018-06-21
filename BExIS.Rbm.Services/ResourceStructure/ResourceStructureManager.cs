﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Persistence.Api;
using BExIS.Rbm.Entities;
using System.Diagnostics.Contracts;
using RS = BExIS.Rbm.Entities.ResourceStructure;
using System.Collections;
using BExIS.Rbm.Entities.ResourceStructure;
using R= BExIS.Rbm.Entities.Resource;

namespace BExIS.Rbm.Services.ResourceStructure
{
   public class ResourceStructureManager
    {
       public ResourceStructureManager()
       {
           IUnitOfWork uow = this.GetUnitOfWork();
           this.ResourceStructureRepo = uow.GetReadOnlyRepository<RS.ResourceStructure>();
           this.ResourceRepo = uow.GetReadOnlyRepository<R.SingleResource>();
       }

       #region ResourceStructure

       public RS.ResourceStructure Create(string name, string description, List<RS.ResourceAttributeUsage> resourceAttributeUsage, RS.ResourceStructure parent)
       {
           Contract.Requires(!string.IsNullOrWhiteSpace(name));
           Contract.Requires(!String.IsNullOrWhiteSpace(description));

           RS.ResourceStructure resourceStructure = new RS.ResourceStructure()
           {
               ResourceAttributeUsages = resourceAttributeUsage,
               Parent = parent,
               Name = name,
               Description = description
           };

           using (IUnitOfWork uow = this.GetUnitOfWork())
           {
               IRepository<RS.ResourceStructure> repo = uow.GetRepository<RS.ResourceStructure>();
               repo.Put(resourceStructure);
               uow.Commit();
           }

           return (resourceStructure);
       }

       public bool Delete(RS.ResourceStructure resoureStruc)
       {
           Contract.Requires(resoureStruc != null);
           Contract.Requires(resoureStruc.Id >= 0);
           ResourceStructureAttributeManager rsaManager = new ResourceStructureAttributeManager();

           bool deleted = rsaManager.DeleteUsagesByRSId(resoureStruc.Id);

           if (deleted)
           {

               using (IUnitOfWork uow = this.GetUnitOfWork())
               {
                   IRepository<RS.ResourceStructure> repoStruc = uow.GetRepository<RS.ResourceStructure>();

                   resoureStruc = repoStruc.Reload(resoureStruc);
                   repoStruc.Delete(resoureStruc);

                   uow.Commit();
               }
               return (true);
           }

           return false;
       }

       public bool Delete(IEnumerable<RS.ResourceStructure> resoureStrucs)
       {
           using (IUnitOfWork uow = this.GetUnitOfWork())
           {
               IRepository<RS.ResourceStructure> repo = uow.GetRepository<RS.ResourceStructure>();
               foreach (var resourceStruc in resoureStrucs)
               {
                   var latest = repo.Reload(resourceStruc);
                   repo.Delete(latest);
               }

               uow.Commit();
           }

           return (true);
       }

       public RS.ResourceStructure Update(RS.ResourceStructure resourceStruc)
       {
           Contract.Requires(resourceStruc != null);

           using (IUnitOfWork uow = this.GetUnitOfWork())
           {
               IRepository<RS.ResourceStructure> repo = uow.GetRepository<RS.ResourceStructure>();
               repo.Put(resourceStruc);
               uow.Commit();
           }

           return resourceStruc;
       }

       public IQueryable<RS.ResourceStructure> GetAllResourceStructures()
       {
           return (ResourceStructureRepo.Query());
       }

       public RS.ResourceStructure GetResourceStructureById(long id)
       {
           return ResourceStructureRepo.Query(u => u.Id == id).FirstOrDefault();
       }

        public RS.ResourceStructure GetResourceStructureByName(string name)
        {
            return ResourceStructureRepo.Query(u => u.Name.ToLower() == name.ToLower()).FirstOrDefault();
        }

        public IQueryable<RS.ResourceStructure> GetResourceStructuresFromResource(long rId)
       {
           return ResourceStructureRepo.Query(rs => rs.Resources.Any(r => r.Id == rId));
       }
       
       public IQueryable<R.Resource> GetRSFromResource(long rsId)
       {
           return ResourceRepo.Query(r => r.ResourceStructure.Id == rsId);
       }


       public bool IsResourceStructureInUse(long rsId)
       {
           IQueryable<R.Resource> rList = this.GetRSFromResource(rsId);
           if (rList.Count() > 0)
               return true;
           else
               return false;
       }

        #endregion

        #region Data Readers

        public IReadOnlyRepository<RS.ResourceStructure> ResourceStructureRepo { get; private set; }
        public IReadOnlyRepository<R.SingleResource> ResourceRepo { get; private set; }

        #endregion

    }
}
