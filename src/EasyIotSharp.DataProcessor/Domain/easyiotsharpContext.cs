using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace EasyIotSharp.DataProcessor.Domain
{
    public partial class easyiotsharpContext : DbContext
    {
        public easyiotsharpContext()
        {
        }

        public easyiotsharpContext(DbContextOptions<easyiotsharpContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Classification> Classification { get; set; }
        public virtual DbSet<Gateway> Gateway { get; set; }
        public virtual DbSet<Gatewayprotocol> Gatewayprotocol { get; set; }
        public virtual DbSet<Gatewayprotocolconfig> Gatewayprotocolconfig { get; set; }
        public virtual DbSet<Menus> Menus { get; set; }
        public virtual DbSet<Projectbase> Projectbase { get; set; }
        public virtual DbSet<Protocol> Protocol { get; set; }
        public virtual DbSet<Protocolconfig> Protocolconfig { get; set; }
        public virtual DbSet<Protocolconfigext> Protocolconfigext { get; set; }
        public virtual DbSet<RabbitProject> RabbitProject { get; set; }
        public virtual DbSet<RabbitServerinfo> RabbitServerinfo { get; set; }
        public virtual DbSet<Rolemenus> Rolemenus { get; set; }
        public virtual DbSet<Roles> Roles { get; set; }
        public virtual DbSet<Sensor> Sensor { get; set; }
        public virtual DbSet<Sensorpoint> Sensorpoint { get; set; }
        public virtual DbSet<Sensorquota> Sensorquota { get; set; }
        public virtual DbSet<Soldierroles> Soldierroles { get; set; }
        public virtual DbSet<Soldiers> Soldiers { get; set; }
        public virtual DbSet<Tenants> Tenants { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseMySql("server=82.156.125.76;port=3306;database=easyiotsharp;user=root;password=jichang2025!@#", x => x.ServerVersion("8.0.16-mysql"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Classification>(entity =>
            {
                entity.ToTable("classification");

                entity.HasComment("分类表");

                entity.Property(e => e.Id)
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.CreationTime).HasColumnType("datetime");

                entity.Property(e => e.DeleteTime).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("分类名称")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.OperatorId)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.OperatorName)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ProjectId)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("项目id")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Sort)
                    .HasColumnType("int(11)")
                    .HasComment(@"排序字段
            字段越大越靠前");

                entity.Property(e => e.TenantNumId)
                    .HasColumnType("int(11)")
                    .HasComment("租户id");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<Gateway>(entity =>
            {
                entity.ToTable("gateway");

                entity.HasComment("网关设备表");

                entity.Property(e => e.Id)
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.CreationTime).HasColumnType("datetime");

                entity.Property(e => e.DeleteTime).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("设备名称")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.OperatorId)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.OperatorName)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ProjectId)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("项目id")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ProtocolId)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("协议id")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.State)
                    .HasColumnType("int(11)")
                    .HasComment(@"设备状态
            0=初始化
            1=在线
            2=离线");

                entity.Property(e => e.TenantNumId)
                    .HasColumnType("int(11)")
                    .HasComment("租户id");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<Gatewayprotocol>(entity =>
            {
                entity.ToTable("gatewayprotocol");

                entity.HasComment("网关协议表");

                entity.Property(e => e.Id)
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ConfigJson)
                    .IsRequired()
                    .HasColumnType("text")
                    .HasComment(@"json定义
            网关发送命令json数据")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.CreationTime).HasColumnType("datetime");

                entity.Property(e => e.DeleteTime).HasColumnType("datetime");

                entity.Property(e => e.GatewayId)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("网关id")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.OperatorId)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.OperatorName)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ProtocolId)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("协议id")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.TenantNumId)
                    .HasColumnType("int(11)")
                    .HasComment("租户id");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<Gatewayprotocolconfig>(entity =>
            {
                entity.ToTable("gatewayprotocolconfig");

                entity.HasComment("网关协议配置表");

                entity.Property(e => e.Id)
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.CreationTime).HasColumnType("datetime");

                entity.Property(e => e.DeleteTime).HasColumnType("datetime");

                entity.Property(e => e.GatewayProtocolId)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("网关协议id")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Identifier)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment(@"标识符
            （反序列化到class里面对应的字段名）")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.OperatorId)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.OperatorName)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<Menus>(entity =>
            {
                entity.ToTable("menus");

                entity.HasComment("租户菜单表");

                entity.Property(e => e.Id)
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.CreationTime).HasColumnType("datetime");

                entity.Property(e => e.DeleteTime).HasColumnType("datetime");

                entity.Property(e => e.Icon)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("菜单图标")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.IsEnable).HasComment("是否启用");

                entity.Property(e => e.IsSuperAdmin).HasComment("是否admin(超级管理员，没有租户限制)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("菜单名称")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.OperatorId)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.OperatorName)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ParentId)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("父级id")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Sort)
                    .HasColumnType("int(11)")
                    .HasComment(@"排序
            (数字越大越靠前）");

                entity.Property(e => e.Type)
                    .HasColumnType("int(11)")
                    .HasComment(@"类型
            1=菜单
            2=路由
            3=按钮");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Url)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("菜单路径")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");
            });

            modelBuilder.Entity<Projectbase>(entity =>
            {
                entity.ToTable("projectbase");

                entity.HasComment("项目表");

                entity.Property(e => e.Id)
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Address)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("项目地址")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.CreationTime).HasColumnType("datetime");

                entity.Property(e => e.DeleteTime).HasColumnType("datetime");

                entity.Property(e => e.Latitude)
                    .IsRequired()
                    .HasColumnName("latitude")
                    .HasColumnType("varchar(255)")
                    .HasComment("纬度")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Longitude)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("经度")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("项目名称")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.OperatorId)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.OperatorName)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Remark)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("项目描述")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.State)
                    .HasColumnType("int(11)")
                    .HasComment(@"项目状态
            0=初始化状态
            1=正在运行状态");

                entity.Property(e => e.TenantNumId)
                    .HasColumnType("int(11)")
                    .HasComment("租户id");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<Protocol>(entity =>
            {
                entity.ToTable("protocol");

                entity.HasComment("协议表");

                entity.Property(e => e.Id)
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.CreationTime).HasColumnType("datetime");

                entity.Property(e => e.DeleteTime).HasColumnType("datetime");

                entity.Property(e => e.IsEnable).HasComment("是否启用");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("协议名称")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.OperatorId)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.OperatorName)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Remark)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("协议描述")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<Protocolconfig>(entity =>
            {
                entity.ToTable("protocolconfig");

                entity.HasComment("协议表单配置表");

                entity.Property(e => e.Id)
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.CreationTime).HasColumnType("datetime");

                entity.Property(e => e.DeleteTime).HasColumnType("datetime");

                entity.Property(e => e.Identifier)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment(@"标识符
            （反序列化到class里面对应的字段名）")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.IsRequired)
                    .HasColumnType("int(11)")
                    .HasComment("是否必填");

                entity.Property(e => e.Label)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("标签")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.OperatorId)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.OperatorName)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Placeholder)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("文本提示")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ProtocolId)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("协议id")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Sort)
                    .HasColumnType("int(11)")
                    .HasComment(@"排序字段
            (数字越大越靠前)");

                entity.Property(e => e.Tag)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("标签")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.TagType)
                    .HasColumnType("int(11)")
                    .HasComment("标签类型  select、number、text");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.ValidateMessage)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("验证数据类型不正确提示")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ValidateType)
                    .HasColumnType("int(11)")
                    .HasComment("验证数据类型");
            });

            modelBuilder.Entity<Protocolconfigext>(entity =>
            {
                entity.ToTable("protocolconfigext");

                entity.HasComment("协议配置扩展表");

                entity.Property(e => e.Id)
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.CreationTime).HasColumnType("datetime");

                entity.Property(e => e.DeleteTime).HasColumnType("datetime");

                entity.Property(e => e.Label)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("名字")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.OperatorId)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.OperatorName)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ProtocolConfigId)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("协议配置id")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("值")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");
            });

            modelBuilder.Entity<RabbitProject>(entity =>
            {
                entity.ToTable("rabbit_project");

                entity.HasComment("配置项目关系表");

                entity.Property(e => e.Id)
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.CreationTime).HasColumnType("datetime");

                entity.Property(e => e.DeleteTime).HasColumnType("datetime");

                entity.Property(e => e.OperatorId)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.OperatorName)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ProjectId)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.RabbitServerInfoId)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<RabbitServerinfo>(entity =>
            {
                entity.ToTable("rabbit_serverinfo");

                entity.HasComment("RabbitMQ服务器配置信息表");

                entity.Property(e => e.Id)
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.CreationTime).HasColumnType("datetime");

                entity.Property(e => e.DeleteTime).HasColumnType("datetime");

                entity.Property(e => e.Exchange)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("交换机名称")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Host)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("主机地址")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.IsEnable).HasComment("是否启用");

                entity.Property(e => e.OperatorId)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.OperatorName)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("密码")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Port)
                    .HasColumnType("int(11)")
                    .HasComment("端口号");

                entity.Property(e => e.TenantNumId)
                    .HasColumnType("int(11)")
                    .HasComment("租户ID");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("用户名")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.VirtualHost)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("虚拟主机")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");
            });

            modelBuilder.Entity<Rolemenus>(entity =>
            {
                entity.ToTable("rolemenus");

                entity.HasComment("角色菜单表");

                entity.Property(e => e.Id)
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.CreationTime).HasColumnType("datetime");

                entity.Property(e => e.DeleteTime).HasColumnType("datetime");

                entity.Property(e => e.MenuId)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("菜单id")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.OperatorId)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.OperatorName)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.RoleId)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("角色id")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.TenantNumId)
                    .HasColumnType("int(11)")
                    .HasComment("租户id");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<Roles>(entity =>
            {
                entity.ToTable("roles");

                entity.HasComment("租户角色表");

                entity.Property(e => e.Id)
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.CreationTime).HasColumnType("datetime");

                entity.Property(e => e.DeleteTime).HasColumnType("datetime");

                entity.Property(e => e.IsEnable).HasComment("是否启用");

                entity.Property(e => e.IsManager)
                    .HasColumnType("int(11)")
                    .HasComment(@"是否管理员（一个租户只有一个用户拥有一个管理员角色）
            1=管理员
            2=普通用户");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("角色名称")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.OperatorId)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.OperatorName)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Remark)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("备注")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.TenantNumId)
                    .HasColumnType("int(11)")
                    .HasComment("租户id");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<Sensor>(entity =>
            {
                entity.ToTable("sensor");

                entity.HasComment("传感器表");

                entity.Property(e => e.Id)
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.BriefName)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("简称")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.CreationTime).HasColumnType("datetime");

                entity.Property(e => e.DeleteTime).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("测点类型名称")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.OperatorId)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.OperatorName)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Remark)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("描述")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.SensorModel)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("传感器型号")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Supplier)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("厂家名称")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.TenantNumId)
                    .HasColumnType("int(11)")
                    .HasComment("租户id");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<Sensorpoint>(entity =>
            {
                entity.ToTable("sensorpoint");

                entity.HasComment("测点表");

                entity.Property(e => e.Id)
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ClassificationId)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("分类id")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.CreationTime).HasColumnType("datetime");

                entity.Property(e => e.DeleteTime).HasColumnType("datetime");

                entity.Property(e => e.GatewayId)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("网关id")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.IsAlarms).HasComment("是否报警");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("测点名称")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.OperatorId)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.OperatorName)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ProjectId)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("项目id")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.SensorId)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("传感器Id")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.State)
                    .HasColumnType("int(11)")
                    .HasComment("状态");

                entity.Property(e => e.TenantNumId)
                    .HasColumnType("int(11)")
                    .HasComment("租户id");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<Sensorquota>(entity =>
            {
                entity.ToTable("sensorquota");

                entity.HasComment("传感器指标表");

                entity.Property(e => e.Id)
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ControlsType)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment(@"操作类型
            R=只读
            W=只写
            RW=读写")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.CreationTime).HasColumnType("datetime");

                entity.Property(e => e.DataType)
                    .HasColumnType("int(11)")
                    .HasComment("数据类型");

                entity.Property(e => e.DeleteTime).HasColumnType("datetime");

                entity.Property(e => e.Identifier)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("指标标识符")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("指标名称")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.OperatorId)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.OperatorName)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Precision)
                    .HasColumnType("int(11)")
                    .HasComment(@"小数位精度
            数据类型是double float  小数点取几位
            -1=全部
            0=零位
            1=一位");

                entity.Property(e => e.Remark)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("描述")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.SensorId)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("传感器类型id")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Sort)
                    .HasColumnType("int(11)")
                    .HasComment(@"排序字段
            (数字越大越靠前)");

                entity.Property(e => e.TenantNumId)
                    .HasColumnType("int(11)")
                    .HasComment("租户id");

                entity.Property(e => e.Unit)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("单位")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<Soldierroles>(entity =>
            {
                entity.ToTable("soldierroles");

                entity.HasComment("用户角色表");

                entity.Property(e => e.Id)
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.CreationTime).HasColumnType("datetime");

                entity.Property(e => e.DeleteTime).HasColumnType("datetime");

                entity.Property(e => e.IsManager)
                    .HasColumnType("int(11)")
                    .HasComment(@"是否管理员（一个租户只有一个用户拥有一个管理员角色）
            1=管理员
            2=普通用户");

                entity.Property(e => e.OperatorId)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.OperatorName)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.RoleId)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("角色id")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.SoldierId)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("用户id")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.TenantNumId)
                    .HasColumnType("int(11)")
                    .HasComment("租户id");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<Soldiers>(entity =>
            {
                entity.ToTable("soldiers");

                entity.HasComment("租户用户表");

                entity.Property(e => e.Id)
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.CreationTime).HasColumnType("datetime");

                entity.Property(e => e.DeleteTime).HasColumnType("datetime");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("邮箱")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.IsEnable).HasComment("是否启用");

                entity.Property(e => e.IsManager)
                    .HasColumnType("int(11)")
                    .HasComment(@"是否管理员
            1=管理员
            2=普通用户");

                entity.Property(e => e.IsSuperAdmin).HasComment("是否admin(超级管理员，没有租户限制)");

                entity.Property(e => e.IsTest).HasComment("是否测试");

                entity.Property(e => e.LastLoginTime)
                    .HasColumnType("datetime")
                    .HasComment("最后一次登录时间");

                entity.Property(e => e.Mobile)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("手机号")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.OperatorId)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.OperatorName)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("密码")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Sex)
                    .HasColumnType("int(11)")
                    .HasComment("姓别：1=男，2=女，-1 = 不选");

                entity.Property(e => e.TenantNumId)
                    .HasColumnType("int(11)")
                    .HasComment("租户numId");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("用户名")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");
            });

            modelBuilder.Entity<Tenants>(entity =>
            {
                entity.ToTable("tenants");

                entity.HasComment("租户表");

                entity.Property(e => e.Id)
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ContractEndTime)
                    .HasColumnType("datetime")
                    .HasComment("合同结束时间");

                entity.Property(e => e.ContractName)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("合同名称")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ContractOwnerMobile)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("合同所属人电话")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ContractOwnerName)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("合同所属人")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ContractStartTime)
                    .HasColumnType("datetime")
                    .HasComment("合同开始时间");

                entity.Property(e => e.CreationTime).HasColumnType("datetime");

                entity.Property(e => e.DeleteTime).HasColumnType("datetime");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("机构的电子邮箱")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.FreezeDes)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("冻结描述")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.IsFreeze).HasComment("是否冻结");

                entity.Property(e => e.ManagerId)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("机构的系统管理员Id 详见")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Mobile)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("机构的负责人手机号")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(50)")
                    .HasComment("租户名称")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.NumId)
                    .HasColumnType("int(11)")
                    .HasComment("numId");

                entity.Property(e => e.OperatorId)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.OperatorName)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Owner)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("机构的负责人姓名")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Remark)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("机构的备注")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.StoreKey)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("机构的（唯一的等同Id）")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.StoreLogoUrl)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasComment("机构的Logo url")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.VersionTypeId)
                    .HasColumnType("int(11)")
                    .HasComment(@"版本类型，详见 
            1=入门版
            2=基础版
            3=旗舰版
            5=全国版");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
