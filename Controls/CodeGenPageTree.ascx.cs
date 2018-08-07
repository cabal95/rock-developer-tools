using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotLiquid;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_blueboxmoon.DeveloperTools
{
    [DisplayName( "CodeGen Page Tree" )]
    [Category( "Blue Box Moon > Developer Tools" )]
    [Description( "Generate code for creating page trees." )]

    [CodeEditorField( "Lava Template", "The lava template when rendering the C# code.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"{% include '~/Plugins/com_blueboxmoon/DeveloperTools/Assets/PageTree.Lava' %}", order: 0 )]
    public partial class CodeGenPageTree : RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbGenerate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbGenerate_Click( object sender, EventArgs e )
        {
            var page = new PageService( new RockContext() ).Get( ppPage.SelectedValueAsId().Value );
            var pages = PageAndDescendants( page );

            string lavaTemplate = GetAttributeValue( "LavaTemplate" );
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( RockPage );
            mergeFields.Add( "RootPage", page );
            mergeFields.Add( "Pages", pages );
            mergeFields.Add( "ReversePages", pages );

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

        protected List<Rock.Model.Page> PageAndDescendants( Rock.Model.Page page )
        {
            var pages = new List<Rock.Model.Page>();

            pages.Add( page );

            foreach ( var child in page.Pages )
            {
                pages.AddRange( PageAndDescendants( child ) );
            }

            return pages;
        }

        #endregion

        public class LavaAdditions
        {
            public static IEnumerable<Rock.Model.Page> PageTree( object input, object rootPageObj )
            {
                var pages = new List<Rock.Model.Page>();
                var page = input as Rock.Model.Page;
                var rootPage = rootPageObj as Rock.Model.Page;
                int? rootPageId = null;

                if ( rootPage != null )
                {
                    rootPageId = rootPage.Id;
                }

                while ( page != null )
                {
                    pages.Add( page );
                    if ( rootPageId.HasValue && page.Id == rootPageId.Value )
                    {
                        break;
                    }

                    page = page.ParentPage;
                }

                return pages;
            }

            /// <summary>
            /// Returns an array that is in reverse order.
            /// </summary>
            /// <param name="input"></param>
            /// <returns></returns>
            public static IEnumerable Reverse( object input )
            {
                if ( input is IEnumerable )
                    return ( ( IEnumerable ) input ).Cast<object>().Reverse();

                return null;
            }

            public static IEnumerable BlockAttributes( object input )
            {
                var block = input as Rock.Model.Block;
                if ( block == null )
                {
                    return new List<AttributeValue>();
                }

                return new AttributeValueService( new RockContext() ).Queryable().AsNoTracking()
                    .Where( v => v.Attribute.EntityTypeQualifierColumn == "BlockTypeId" && v.Attribute.EntityTypeQualifierValue == block.BlockType.Id.ToString() )
                    .Where( v => v.EntityId == block.Id )
                    .ToList();
            }
        }
    }
}