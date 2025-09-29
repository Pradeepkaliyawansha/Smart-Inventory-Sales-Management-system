using AutoMapper;
using InventoryAPI.Models.DTOs;
using InventoryAPI.Models.Entities; // ASSUMPTION: Your entity models are here

namespace InventoryAPI.Mappings
{
    // ASSUMPTION: This configuration relies on the existence of Entity classes 
    // named User, Customer, Product, Category, Supplier, Sale, SaleItem, and StockMovement
    // within the InventoryAPI.Models.Entities namespace.
    
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // =======================================================
            // 1. AUTH MAPPINGS (Resolves the original 500 error)
            // =======================================================
            
            // RegisterDto -> User (Create)
            CreateMap<RegisterDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.LastLogin, opt => opt.Ignore())
                .ForMember(dest => dest.RefreshToken, opt => opt.Ignore())
                .ForMember(dest => dest.RefreshTokenExpiry, opt => opt.Ignore());

            // User <-> UserDto (Read/Response)
            CreateMap<User, UserDto>().ReverseMap();
            
            // =======================================================
            // 2. CUSTOMER MAPPINGS
            // =======================================================
            
            // CreateCustomerDto -> Customer
            CreateMap<CreateCustomerDto, Customer>();
            
            // UpdateCustomerDto -> Customer
            CreateMap<UpdateCustomerDto, Customer>();
            
            // Customer <-> CustomerDto
            CreateMap<Customer, CustomerDto>().ReverseMap();
            
            // =======================================================
            // 3. PRODUCT MAPPINGS
            // =======================================================
            
            // CreateProductDto -> Product
            CreateMap<CreateProductDto, Product>();
            
            // UpdateProductDto -> Product
            CreateMap<UpdateProductDto, Product>();
            
            // Product <-> ProductDto
            CreateMap<Product, ProductDto>().ReverseMap();
            
            // StockMovement -> StockMovementDto
            CreateMap<StockMovement, StockMovementDto>().ReverseMap();
            
            // =======================================================
            // 4. CATEGORY MAPPINGS
            // =======================================================
            
            // CreateCategoryDto -> Category
            CreateMap<CreateCategoryDto, Category>();
            
            // UpdateCategoryDto -> Category
            CreateMap<UpdateCategoryDto, Category>();
            
            // Category <-> CategoryDto
            CreateMap<Category, CategoryDto>().ReverseMap();
            
            // =======================================================
            // 5. SUPPLIER MAPPINGS
            // =======================================================
            
            // CreateSupplierDto -> Supplier
            CreateMap<CreateSupplierDto, Supplier>();
            
            // UpdateSupplierDto -> Supplier
            CreateMap<UpdateSupplierDto, Supplier>();
            
            // Supplier <-> SupplierDto
            CreateMap<Supplier, SupplierDto>().ReverseMap();

            // =======================================================
            // 6. SALE MAPPINGS
            // =======================================================
            
            // CreateSaleDto -> Sale
            CreateMap<CreateSaleDto, Sale>()
                .ForMember(dest => dest.SaleItems, opt => opt.Ignore()) // Items mapped separately
                .ForMember(dest => dest.UserId, opt => opt.Ignore()); // UserId set by controller/service
            
            // Sale <-> SaleDto
            CreateMap<Sale, SaleDto>().ReverseMap();
            
            // CreateSaleItemDto -> SaleItem
            CreateMap<CreateSaleItemDto, SaleItem>();
            
            // SaleItem <-> SaleItemDto
            CreateMap<SaleItem, SaleItemDto>().ReverseMap();
            
            // =======================================================
            // 7. BULK OPERATION MAPPINGS (Command-to-Entity Updates)
            // =======================================================
            
            // BulkSupplierStatusUpdateDto -> Supplier (for updating IsActive/Reason)
            CreateMap<BulkSupplierStatusUpdateDto, Supplier>();
            
            // BulkCategoryStatusUpdateDto -> Category (for updating IsActive/Reason)
            CreateMap<BulkCategoryStatusUpdateDto, Category>();

            // NOTE: DTOs like LoginDto, AuthResponseDto, and all Report DTOs 
            // are command/response objects and do not require mappings to core entities.
        }
    }
}