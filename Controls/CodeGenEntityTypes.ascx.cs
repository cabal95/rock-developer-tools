using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_blueboxmoon.DeveloperTools
{
    [DisplayName( "CodeGen Entity Types" )]
    [Category( "Blue Box Moon > Developer Tools" )]
    [Description( "Generate code for creating entity types." )]

    [CodeEditorField( "Lava Template", "The lava template when rendering the C# code.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"{% include '~/Plugins/com_blueboxmoon/DeveloperTools/Assets/EntityTypes.Lava' %}", order: 0 )]
    public partial class CodeGenEntityTypes : RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gEntityTypes.RowItemText = "Entity Type";
            gEntityTypes.DataKeyNames = new string[] { "Id" };
            gEntityTypes.Actions.ShowAdd = false;
            gEntityTypes.GridRebind += gEntityTypes_GridRebind;
            gEntityTypes.IsDeleteEnabled = false;

            var lbGenerate = new LinkButton
            {
                ID = "lbGenerate",
                CssClass = "btn btn-default btn-sm",
                Text = "<i class='fa fa-code'></i>"
            };
            lbGenerate.Click += lbGenerate_Click;
            gEntityTypes.Actions.Controls.Add( lbGenerate );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                tbNameFilter.Text = gfSettings.GetUserPreference( "Name" );
                ddlIsEntityFilter.SelectedValue = gfSettings.GetUserPreference( "IsEntity" );
                ddlIsSecuredFilter.SelectedValue = gfSettings.GetUserPreference( "IsSecured" );

                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Apply Filter event for the GridFilter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void gfSettings_ApplyFilterClick( object sender, EventArgs e )
        {
            gfSettings.SaveUserPreference( "Name", tbNameFilter.Text );
            gfSettings.SaveUserPreference( "IsEntity", ddlIsEntityFilter.SelectedValue );
            gfSettings.SaveUserPreference( "IsSecured", ddlIsSecuredFilter.SelectedValue );

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gEntityTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gEntityTypes_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Click event of the lbGenerate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void lbGenerate_Click( object sender, EventArgs e )
        {
            var entityTypes = GetQuery().ToList();
            var ids = gEntityTypes.SelectedKeys.Cast<int>().ToList();

            if ( ids.Any() )
            {
                entityTypes = entityTypes.Where( b => ids.Contains( b.Id ) ).ToList();
            }

            string lavaTemplate = GetAttributeValue( "LavaTemplate" );
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( RockPage );
            mergeFields.Add( "EntityTypes", entityTypes );

            DotLiquid.Template template = DotLiquid.Template.Parse( lavaTemplate );
            DotLiquid.RenderParameters parameters = new DotLiquid.RenderParameters
            {
                LocalVariables = DotLiquid.Hash.FromDictionary( mergeFields ),
                Filters = new Type[] { }
            };
            lResults.Text = template.Render( parameters ).EncodeHtml();
            pResults.Visible = true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the query.
        /// </summary>
        /// <returns></returns>
        private IQueryable<EntityType> GetQuery()
        {
            var entityTypeService = new EntityTypeService( new RockContext() );

            var entityTypes = entityTypeService.Queryable().AsNoTracking();

            // Filter by Name
            string nameFilter = gfSettings.GetUserPreference( "Name" );
            if ( !string.IsNullOrEmpty( nameFilter.Trim() ) )
            {
                entityTypes = entityTypes.Where( a => a.Name.Contains( nameFilter.Trim() ) );
            }

            // Filter by Is Entity
            bool? isEntityFilter = gfSettings.GetUserPreference( "IsEntity" ).AsBooleanOrNull();
            if ( isEntityFilter.HasValue )
            {
                entityTypes = entityTypes.Where( a => a.IsEntity == isEntityFilter.Value );
            }

            // Filter by Is Entity
            bool? isSecuredFilter = gfSettings.GetUserPreference( "IsEntity" ).AsBooleanOrNull();
            if ( isSecuredFilter.HasValue )
            {
                entityTypes = entityTypes.Where( a => a.IsEntity == isSecuredFilter.Value );
            }

            return entityTypes;
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            SortProperty sortProperty = gEntityTypes.SortProperty;
            var entityTypes = GetQuery();

            var selectQry = entityTypes.Select( a =>
                new
                {
                    a.Id,
                    a.Name,
                    a.IsEntity,
                    a.IsSecured
                } );

            if ( sortProperty != null )
            {
                gEntityTypes.DataSource = selectQry.Sort( sortProperty ).ToList();
            }
            else
            {
                gEntityTypes.DataSource = selectQry.OrderBy( b => b.Name ).ToList();
            }

            gEntityTypes.EntityTypeId = new BlockType().TypeId;
            gEntityTypes.DataBind();
        }

        #endregion
    }
}