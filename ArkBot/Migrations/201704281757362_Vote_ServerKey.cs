namespace ArkBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Vote_ServerKey : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Votes", "ServerKey", c => c.String(maxLength: 4000));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Votes", "ServerKey");
        }
    }
}
