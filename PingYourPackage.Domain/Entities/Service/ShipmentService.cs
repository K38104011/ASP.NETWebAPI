using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PingYourPackage.Domain.Entities.Service
{
    public class ShipmentService : IShipmentService
    {
        private readonly IEntityRepository<ShipmentType> _shipmentTypeRepository;
        private readonly IEntityRepository<Shipment> _shipmentRepository;
        private readonly IEntityRepository<ShipmentState> _shipmentStateRepository;
        private readonly IEntityRepository<Affiliate> _affiliateRepository;
        private readonly IMembershipService _membershipService;

        public ShipmentService(
            IEntityRepository<ShipmentType> shipmentTypeRepository,
            IEntityRepository<Shipment> shipmentRepository,
            IEntityRepository<ShipmentState> shipmentStateRepository,
            IEntityRepository<Affiliate> affiliateRepository,
            IMembershipService membershipService)
        {
            _shipmentTypeRepository = shipmentTypeRepository;
            _shipmentRepository = shipmentRepository;
            _shipmentStateRepository = shipmentStateRepository;
            _affiliateRepository = affiliateRepository;
            _membershipService = membershipService;
        }

        public PaginatedList<ShipmentType> GetShipmentTypes(int pageIndex, int pageSize)
        {
            var shipmentTypes = _shipmentTypeRepository.
                Paginate(pageIndex, pageSize, x => x.CreatedOn);

            return shipmentTypes;
        }

        public ShipmentType GetShipmentType(Guid key)
        {
            var shipmentType = _shipmentTypeRepository.GetSingle(key);
            return shipmentType;
        }

        public OperationResult<ShipmentType> AddShipmentType(ShipmentType shipmentType)
        {
            if (_shipmentTypeRepository.GetSingleByName(shipmentType.Name) != null)
            {
                return new OperationResult<ShipmentType>(false);
            }

            shipmentType.Key = Guid.NewGuid();
            shipmentType.CreatedOn = DateTime.Now;

            _shipmentTypeRepository.Add(shipmentType);
            _shipmentTypeRepository.Save();

            return new OperationResult<ShipmentType>(true)
            {
                Entity = shipmentType
            };
        }

        public ShipmentType UpdateShipmentType(ShipmentType shipmentType)
        {
            _shipmentTypeRepository.Edit(shipmentType);
            _shipmentTypeRepository.Save();

            return shipmentType;
        }

        public PaginatedList<Affiliate> GetAffiliates(int pageIndex, int pageSize)
        {
            var affiliates = _affiliateRepository
                .AllIncluding(x => x.User).OrderBy(x => x.CreatedOn)
                .ToPaginatedList(pageIndex, pageSize);

            return affiliates;
        }

        public Affiliate GetAffiliate(Guid key)
        {
            var affiliate = _affiliateRepository
                .AllIncluding(x => x.User)
                .FirstOrDefault(x => x.Key == key);

            return affiliate;
        }

        public OperationResult<Affiliate> AddAffiliate(Guid userKey, Affiliate affiliate)
        {
            var userResult = _membershipService.GetUser(userKey);
            if (userResult == null ||
                !userResult.Roles.Any(role => role.Name.Equals(
                    "affiliate", StringComparison.OrdinalIgnoreCase)) ||
                _affiliateRepository.GetSingle(userKey) != null)
            {
                return new OperationResult<Affiliate>(false);
            }

            affiliate.Key = userKey;
            affiliate.CreatedOn = DateTime.Now;

            _affiliateRepository.Add(affiliate);
            _affiliateRepository.Save();

            affiliate.User = userResult.User;
            return new OperationResult<Affiliate>(true)
            {
                Entity = affiliate
            };
        }

        public Affiliate UpdateAffiliate(Affiliate affiliate)
        {
            _affiliateRepository.Edit(affiliate);
            _affiliateRepository.Save();

            return affiliate;
        }

        public PaginatedList<Shipment> GetShipments(int pageIndex, int pageSize)
        {
            var shipments = GetInitialShipments()
                .ToPaginatedList(pageIndex, pageSize);

            return shipments;
        }

        public PaginatedList<Shipment> GetShipments(int pageIndex, int pageSize, Guid affiliateKey)
        {
            var shipments = _shipmentRepository
                .GetShipmentsByAffiliateKey(affiliateKey)
                .OrderBy(x => x.CreateOn)
                .ToPaginatedList(pageIndex, pageSize);

            return shipments;
        }

        public Shipment GetShipment(Guid key)
        {
            var shipment = GetInitialShipments()
                .FirstOrDefault(x => x.Key == key);

            return shipment;
        }

        public OperationResult<Shipment> AddShipment(Shipment shipment)
        {
            var affiliate = _affiliateRepository.GetSingle(shipment.AffiliateKey);
            var shipmentType = _shipmentTypeRepository.GetSingle(shipment.ShipmentTypeKey);

            if (affiliate == null || shipmentType == null)
            {
                return new OperationResult<Shipment>(false);
            }

            shipment.Key = Guid.NewGuid();
            shipment.CreateOn = DateTime.Now;

            _shipmentRepository.Add(shipment);
            _shipmentRepository.Save();

            var shipmentState = InsertFirstShipmentState(shipment.Key);
            shipment.ShipmentType = shipmentType;
            shipment.ShipmentStates = new List<ShipmentState> { shipmentState };

            return new OperationResult<Shipment>(true)
            {
                Entity = shipment
            };
        }

        public Shipment UpdateShipment(Shipment shipment)
        {
            _shipmentRepository.Edit(shipment);
            _shipmentRepository.Save();

            var updatedShipment = GetShipment(shipment.Key);

            return updatedShipment;
        }

        public OperationResult RemoveShipment(Shipment shipment)
        {
            if (IsShipmentRemovable(shipment))
            {
                _shipmentRepository.DeleteGraph(shipment);
                _shipmentRepository.Save();

                return new OperationResult(true);
            }

            return new OperationResult(false);
        }

        public IEnumerable<ShipmentState> GetShipmentStates(Guid shipmentKey)
        {
            var shipmentStates = _shipmentStateRepository.GetAllByShipmentKey(shipmentKey);
            return shipmentStates;
        }

        public OperationResult<ShipmentState> AddShipmentState(Guid shipmentKey, ShipmentStatus status)
        {
            if (!IsShipmentStateInsertable(shipmentKey, status))
            {
                return new OperationResult<ShipmentState>(false);
            }

            var shipmentState = InsertShipmentState(shipmentKey, status);

            return new OperationResult<ShipmentState>(true)
            {
                Entity = shipmentState
            };
        }

        public bool isAffiliateRelatedToUser(Guid affiliateKey, string username)
        {
            var affiliate = GetAffiliate(affiliateKey);

            return affiliate != null &&
                affiliate.User.Name.Equals(username);
        }

        // private helpers

        private IQueryable<Shipment> GetInitialShipments()
        {
            return _shipmentRepository.AllIncluding(
                x => x.ShipmentType, x => x.ShipmentStates).OrderBy(x => x.CreateOn);
        }

        private ShipmentState InsertFirstShipmentState(Guid shipmentKey)
        {
            return InsertShipmentState(shipmentKey, ShipmentStatus.Ordered);
        }

        private ShipmentState InsertShipmentState(Guid shipmentKey, ShipmentStatus status)
        {
            var shipmentState = new ShipmentState
            {
                Key = Guid.NewGuid(),
                ShipmentKey = shipmentKey,
                ShipmentStatus = status,
                CreatedOn = DateTime.Now
            };

            _shipmentStateRepository.Add(shipmentState);
            _shipmentStateRepository.Save();

            return shipmentState;
        }

        private bool IsShipmentStateInsertable(Guid shipmentKey, ShipmentStatus status)
        {
            var shipmentStates = GetShipmentStates(shipmentKey);
            var latestState = (from state in shipmentStates
                               orderby state.ShipmentStatus descending
                               select state).First();

            return status > latestState.ShipmentStatus;
        }

        private bool IsShipmentRemovable(Shipment shipment)
        {
            var latestStatus = (from shipmentState in shipment.ShipmentStates.ToList()
                                orderby shipmentState.ShipmentStatus descending
                                select shipmentState).First();

            return latestStatus.ShipmentStatus < ShipmentStatus.InTransit;
        }
    }
}
