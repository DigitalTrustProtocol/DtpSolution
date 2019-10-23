using System;
using System.Reflection;
using IdentityServer4.AccessTokenValidation;
using IdentityServer4.EntityFramework.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using IS4Amin.Admin.Api.Configuration;
using IS4Amin.Admin.Api.Configuration.ApplicationParts;
using IS4Amin.Admin.Api.Configuration.Constants;
using IS4Amin.Admin.Api.Helpers.Localization;
using Skoruba.IdentityServer4.Admin.BusinessLogic.Identity.Dtos.Identity;
using Skoruba.IdentityServer4.Admin.EntityFramework.Interfaces;

namespace IS4Amin.Admin.Api.Helpers
{
    public static class StartupHelpers
    {

        /// <summary>
        /// Register services for MVC
        /// </summary>
        /// <param name="services"></param>
        public static void AddMvcServices<TUserDto, TUserDtoKey, TRoleDto, TRoleDtoKey, TUserKey, TRoleKey,
            TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken,
            TUsersDto, TRolesDto, TUserRolesDto, TUserClaimsDto,
            TUserProviderDto, TUserProvidersDto, TUserChangePasswordDto, TRoleClaimsDto>(
            this IServiceCollection services)
            where TUserDto : UserDto<TUserDtoKey>, new()
            where TRoleDto : RoleDto<TRoleDtoKey>, new()
            where TUser : IdentityUser<TKey>
            where TRole : IdentityRole<TKey>
            where TKey : IEquatable<TKey>
            where TUserClaim : IdentityUserClaim<TKey>
            where TUserRole : IdentityUserRole<TKey>
            where TUserLogin : IdentityUserLogin<TKey>
            where TRoleClaim : IdentityRoleClaim<TKey>
            where TUserToken : IdentityUserToken<TKey>
            where TRoleDtoKey : IEquatable<TRoleDtoKey>
            where TUserDtoKey : IEquatable<TUserDtoKey>
            where TUsersDto : UsersDto<TUserDto, TUserDtoKey>
            where TRolesDto : RolesDto<TRoleDto, TRoleDtoKey>
            where TUserRolesDto : UserRolesDto<TRoleDto, TUserDtoKey, TRoleDtoKey>
            where TUserClaimsDto : UserClaimsDto<TUserDtoKey>
            where TUserProviderDto : UserProviderDto<TUserDtoKey>
            where TUserProvidersDto : UserProvidersDto<TUserDtoKey>
            where TUserChangePasswordDto : UserChangePasswordDto<TUserDtoKey>
            where TRoleClaimsDto : RoleClaimsDto<TRoleDtoKey>
        {
            services.TryAddTransient(typeof(IGenericControllerLocalizer<>), typeof(GenericControllerLocalizer<>));

            services.AddMvc(o =>
                {
                    o.Conventions.Add(new GenericControllerRouteConvention());
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddDataAnnotationsLocalization()
                .ConfigureApplicationPartManager(m =>
                {
                    m.FeatureProviders.Add(new GenericTypeControllerFeatureProvider<TUserDto, TUserDtoKey, TRoleDto, TRoleDtoKey, TUserKey, TRoleKey, TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken,
                        TUsersDto, TRolesDto, TUserRolesDto, TUserClaimsDto,
                        TUserProviderDto, TUserProvidersDto, TUserChangePasswordDto, TRoleClaimsDto>());
                });
        }


        /// <summary>
        /// Register DbContexts for IdentityServer ConfigurationStore and PersistedGrants, Identity and Logging
        /// Configure the connection strings in AppSettings.json
        /// </summary>
        /// <typeparam name="TConfigurationDbContext"></typeparam>
        /// <typeparam name="TPersistedGrantDbContext"></typeparam>
        /// <typeparam name="TLogDbContext"></typeparam>
        /// <typeparam name="TIdentityDbContext"></typeparam>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddDbContexts<TIdentityDbContext, TConfigurationDbContext, TPersistedGrantDbContext, TLogDbContext>(this IServiceCollection services, IConfiguration configuration)
        where TIdentityDbContext : DbContext
        where TPersistedGrantDbContext : DbContext, IAdminPersistedGrantDbContext
        where TConfigurationDbContext : DbContext, IAdminConfigurationDbContext
        where TLogDbContext : DbContext, IAdminLogDbContext
        {
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            services.AddDbContext<TIdentityDbContext>(options => {
                var provider = configuration.GetValue<string>(ConfigurationConsts.DatabaseProvider);
                var connectionString = configuration.GetConnectionString(ConfigurationConsts.IdentityDbConnectionStringKey);
                if ("Sqlite".Equals(provider, StringComparison.OrdinalIgnoreCase))
                    options.UseSqlite(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
                else
                    options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
            });


            // Config DB from existing connection
            services.AddConfigurationDbContext<TConfigurationDbContext>(options =>
            {

                options.ConfigureDbContext = b => {
                    var provider = configuration.GetValue<string>(ConfigurationConsts.DatabaseProvider);
                    var connectionString = configuration.GetConnectionString(ConfigurationConsts.ConfigurationDbConnectionStringKey);
                    if ("Sqlite".Equals(provider, StringComparison.OrdinalIgnoreCase))
                        b.UseSqlite(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
                    else
                        b.UseSqlServer(connectionString,
                            sql => sql.MigrationsAssembly(migrationsAssembly));
                };
            });

            // Operational DB from existing connection
            services.AddOperationalDbContext<TPersistedGrantDbContext>(options =>
            {
                options.ConfigureDbContext = b =>
                {
                    var provider = configuration.GetValue<string>(ConfigurationConsts.DatabaseProvider);
                    var connectionString = configuration.GetConnectionString(ConfigurationConsts.PersistedGrantDbConnectionStringKey);
                    if ("Sqlite".Equals(provider, StringComparison.OrdinalIgnoreCase))
                        b.UseSqlite(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
                    else
                        b.UseSqlServer(connectionString,
                    sql => sql.MigrationsAssembly(migrationsAssembly));
                };
            });

            // Log DB from existing connection
            services.AddDbContext<TLogDbContext>(options =>
            {
                var provider = configuration.GetValue<string>(ConfigurationConsts.DatabaseProvider);
                var connectionString = configuration.GetConnectionString(ConfigurationConsts.AdminLogDbConnectionStringKey);
                if ("Sqlite".Equals(provider, StringComparison.OrdinalIgnoreCase))
                    options.UseSqlite(connectionString, optionsSql => optionsSql.MigrationsAssembly(migrationsAssembly));
                else
                    options.UseSqlServer(connectionString, optionsSql => optionsSql.MigrationsAssembly(migrationsAssembly));
            });

        }

        /// <summary>
        /// Add authentication middleware for an API
        /// </summary>
        /// <typeparam name="TIdentityDbContext">DbContext for an access to Identity</typeparam>
        /// <typeparam name="TUser">Entity with User</typeparam>
        /// <typeparam name="TRole">Entity with Role</typeparam>
        /// <param name="services"></param>
        /// <param name="adminApiConfiguration"></param>
        public static void AddApiAuthentication<TIdentityDbContext, TUser, TRole>(this IServiceCollection services,
            AdminApiConfiguration adminApiConfiguration) 
            where TIdentityDbContext : DbContext 
            where TRole : class 
            where TUser : class
        {
            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = adminApiConfiguration.IdentityServerBaseUrl;
                    options.ApiName = adminApiConfiguration.OidcApiName;

                    // NOTE: This is only for development set for false
                    // For production use - set RequireHttpsMetadata to true!
                    options.RequireHttpsMetadata = false;
                });

            services.AddIdentity<TUser, TRole>(options =>
                {
                    options.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<TIdentityDbContext>()
                .AddDefaultTokenProviders();
        }

        public static void AddAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(AuthorizationConsts.AdministrationPolicy,
                    policy => policy.RequireRole(AuthorizationConsts.AdministrationRole));
            });
        }
    }
}
