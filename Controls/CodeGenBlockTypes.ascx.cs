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
    [DisplayName( "CodeGen Block Types" )]
    [Category( "Blue Box Moon > Developer Tools" )]
    [Description( "Generate code for creating block types." )]

    [CodeEditorField( "Lava Template", "The lava template when rendering the C# code.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"{% include '~/Plugins/com_blueboxmoon/DeveloperTools/Assets/BlockTypes.Lava' %}", order: 0 )]
    public partial class CodeGenBlockTypes : RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gBlockTypes.RowItemText = "Block Type";
            gBlockTypes.DataKeyNames = new string[] { "Id" };
            gBlockTypes.Actions.ShowAdd = false;
            gBlockTypes.GridRebind += gBlockTypes_GridRebind;
            gBlockTypes.IsDeleteEnabled = false;

            var lbGenerate = new LinkButton
            {
                ID = "lbGenerate",
                CssClass = "btn btn-default btn-sm",
                Text = "<i class='fa fa-code'></i>"
            };
            lbGenerate.Click += lbGenerate_Click;
            gBlockTypes.Actions.Controls.Add( lbGenerate );
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
                tbPathFilter.Text = gfSettings.GetUserPreference( "Path" );

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
            gfSettings.SaveUserPreference( "Path", tbPathFilter.Text );

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gBlockTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gBlockTypes_GridRebind( object sender, EventArgs e )
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
            var blockTypes = GetQuery().ToList();
            var ids = gBlockTypes.SelectedKeys.Cast<int>().ToList();

            if ( ids.Any() )
            {
                blockTypes = blockTypes.Where( b => ids.Contains( b.Id ) ).ToList();
            }

            blockTypes.ForEach( b => b.LoadAttributes() );

            string lavaTemplate = GetAttributeValue( "LavaTemplate" );
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( RockPage );
            mergeFields.Add( "BlockTypes", blockTypes );

            DotLiquid.Template template = DotLiquid.Template.Parse( lavaTemplate );
            DotLiquid.RenderParameters parameters = new DotLiquid.RenderParameters
            {
                LocalVariables = DotLiquid.Hash.FromDictionary( mergeFields ),
                Filters = new Type[] { typeof( LavaAdditions ) }
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
        private IQueryable<BlockType> GetQuery()
        {
            BlockTypeService blockTypeService = new BlockTypeService( new RockContext() );

            var blockTypes = blockTypeService.Queryable().AsNoTracking();

            // Filter by Name
            string nameFilter = gfSettings.GetUserPreference( "Name" );
            if ( !string.IsNullOrEmpty( nameFilter.Trim() ) )
            {
                blockTypes = blockTypes.Where( b => b.Name.Contains( nameFilter.Trim() ) );
            }

            // Filter by Path
            string path = gfSettings.GetUserPreference( "Path" );
            if ( !string.IsNullOrEmpty( path.Trim() ) )
            {
                blockTypes = blockTypes.Where( b => b.Path.Contains( path.Trim() ) );
            }

            return blockTypes;
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            SortProperty sortProperty = gBlockTypes.SortProperty;
            var blockTypes = GetQuery();

            var selectQry = blockTypes.Select( a =>
                new
                {
                    a.Id,
                    a.Name,
                    a.Category,
                    a.Description,
                    a.Path
                } );

            if ( sortProperty != null )
            {
                gBlockTypes.DataSource = selectQry.Sort( sortProperty ).ToList();
            }
            else
            {
                gBlockTypes.DataSource = selectQry.OrderBy( b => b.Name ).ToList();
            }

            gBlockTypes.EntityTypeId = new BlockType().TypeId;
            gBlockTypes.DataBind();
        }

        #endregion

        public class LavaAdditions
        {
            public static IEnumerable<Rock.Model.Attribute> BlockTypeAttributes( object input )
            {
                var blocktype = input as BlockType;
                if (blocktype == null)
                {
                    return new List<Rock.Model.Attribute>();
                }

                return new AttributeService( new RockContext() ).Queryable().AsNoTracking()
                    .Where( a => a.EntityTypeQualifierColumn == "BlockTypeId" && a.EntityTypeQualifierValue == blocktype.Id.ToString() )
                    .ToList();
            }
        }
    }
}