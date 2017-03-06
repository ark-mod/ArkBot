namespace ArkBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TamesLogHealthPercentage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TamedCreatureLogEntries", "ApproxHealthPercentage", c => c.Double());
        }
        
        public override void Down()
        {
            DropColumn("dbo.TamedCreatureLogEntries", "ApproxHealthPercentage");
        }
    }
}
