using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotLiquid;
using Humanizer;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

// ScheduleDetail block is a good template for View+Edit details mode.
namespace RockWeb.Plugins.com_blueboxmoon.DeveloperTools
{
    [DisplayName( "CodeGen Block Template" )]
    [Category( "Blue Box Moon > Developer Tools" )]
    [Description( "Generate code for block templates." )]

    [CodeEditorField( "List Lava Template", "The lava template when rendering the C# code for a List Block.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"{% include '~/Plugins/com_blueboxmoon/DeveloperTools/Assets/BlockItemList.Lava' %}", order: 0 )]
    public partial class CodeGenBlockTemplate : RockBlock
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

            if ( !IsPostBack )
            {
                tbOrganization.Text = GetUserPreference( "Organization" );
                tbDomain.Text = GetUserPreference( "Domain" );
                tbProject.Text = GetUserPreference( "Project" );

                etpEntity.EntityTypes = new EntityTypeService( new RockContext() ).Queryable().AsNoTracking()
                    .Where( a => a.IsEntity ).ToList();
            }
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
            string lavaTemplate = null;

            SetUserPreference( "Organization", tbOrganization.Text );
            SetUserPreference( "Domain", tbDomain.Text );
            SetUserPreference( "Project", tbProject.Text );

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( RockPage );
            mergeFields.Add( "Organization", tbOrganization.Text.Trim() );
            mergeFields.Add( "Domain", tbDomain.Text.Trim() );
            mergeFields.Add( "Project", tbProject.Text.Trim() );

            if ( ddlBlockType.SelectedValue == "Entity List" )
            {
                lavaTemplate = PrepareEntityListFields( mergeFields );
            }

            if ( lavaTemplate == null )
            {
                return;
            }

            var template = Template.Parse( lavaTemplate );
            RenderParameters parameters = new RenderParameters
            {
                LocalVariables = Hash.FromDictionary( mergeFields ),
                Filters = new Type[] { typeof( LavaAdditions ) }
            };

            //
            // Convert the results into seperate "file contents" items.
            //
            var result = template.Render( parameters );
            var files = result.Split( new string[] { "--**--" }, StringSplitOptions.RemoveEmptyEntries )
                .Where( f => f.Trim().Length != 0 )
                .Select( s => new
                {
                    Name = GetFileNameFromSegment( s.Trim() ),
                    Content = GetFileContentFromSegment( s.Trim() ).EncodeHtml()
                } )
                .ToList();


            rptrFileHeaders.DataSource = files;
            rptrFileHeaders.DataBind();
            rptrFiles.DataSource = files;
            rptrFiles.DataBind();
            pnlResults.Visible = true;
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the etpEntity control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void etpEntity_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( !etpEntity.SelectedEntityTypeId.HasValue )
            {
                cblDisplay.Visible = false;
                cblFilter.Visible = false;

                return;
            }

            var entityType = new EntityTypeService( new RockContext() ).Get( etpEntity.SelectedEntityTypeId.Value );

            var type = Type.GetType( entityType.AssemblyName );
            var properties = GetTypeProperties( type )
                .Select( p => p.Name )
                .ToList();

            cblDisplay.Items.Clear();
            cblFilter.Items.Clear();
            foreach ( var property in properties )
            {
                cblDisplay.Items.Add( property );
                cblFilter.Items.Add( property );
            }

            cblDisplay.Visible = true;
            cblFilter.Visible = true;
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlBlockType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlBlockType_SelectedIndexChanged( object sender, EventArgs e )
        {
            pnlEntityListBlock.Visible = false;

            if ( ddlBlockType.SelectedValue == "Entity List" )
            {
                pnlEntityListBlock.Visible = true;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepares the entity list merge fields.
        /// </summary>
        /// <param name="mergeFields">The base merge fields.</param>
        /// <returns>The lava template to use when merging.</returns>
        protected string PrepareEntityListFields( Dictionary<string, object> mergeFields )
        {
            var entityType = new EntityTypeService( new RockContext() ).Get( etpEntity.SelectedEntityTypeId.Value );
            var displayProperties = cblDisplay.SelectedValues;
            var filterProperties = cblFilter.SelectedValues;

            var type = Type.GetType( entityType.AssemblyName );
            var properties = GetTypeProperties( type )
                .Select( a => new
                {
                    a.Name,
                    Type = GetFriendlyTypeName( a.PropertyType ),
                    Display = displayProperties.Contains( a.Name ),
                    Filter = filterProperties.Contains( a.Name )
                } )
                .ToList();

            mergeFields.Add( "Class", entityType.Name.Split( '.' ).Last() );
            mergeFields.Add( "Properties", properties );

            return GetAttributeValue( "ListLavaTemplate" );
        }

        /// <summary>
        /// Gets the file name from code segment.
        /// </summary>
        /// <param name="segment">The code segment.</param>
        /// <returns></returns>
        protected string GetFileNameFromSegment( string segment )
        {
            var regex = new Regex( @"^\[([A-Za-z0-9\._]+)\]" );
            var match = regex.Match( segment );

            if ( match.Success )
            {
                return match.Groups[1].Value;
            }

            return "Unknown";
        }

        /// <summary>
        /// Gets the file content from code segment.
        /// </summary>
        /// <param name="segment">The code segment.</param>
        /// <returns></returns>
        protected string GetFileContentFromSegment( string segment )
        {
            var regex = new Regex( @"^\[([A-Za-z0-9\._]+)\]" );

            return regex.Replace( segment, string.Empty ).Trim();
        }

        /// <summary>
        /// Gets the type properties.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        protected PropertyInfo[] GetTypeProperties( Type type )
        {
            return type.GetProperties( BindingFlags.Instance | BindingFlags.Public )
                .Where( p =>
                    !p.GetGetMethod().IsVirtual ||
                    p.GetCustomAttributes( typeof( IncludeForReportingAttribute ), true ).Any() ||
                    p.Name == "Order" || p.Name == "IsActive" )
                .ToArray();
        }

        /// <summary>
        /// Gets the name of the friendly type. Example, Int32 => 'int'
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        protected string GetFriendlyTypeName( Type type )
        {
            var compiler = new Microsoft.CSharp.CSharpCodeProvider();
            string name;

            if ( Nullable.GetUnderlyingType( type ) != null )
            {
                name = compiler.GetTypeOutput( new System.CodeDom.CodeTypeReference( Nullable.GetUnderlyingType( type ) ) ) + "?";
            }
            else
            {
                name = compiler.GetTypeOutput( new System.CodeDom.CodeTypeReference( type ) );
            }

            return name.Split( '.' ).Last();
        }

        #endregion

        public class LavaAdditions
        {
            public static string FilenameSection( string input )
            {
                return string.Format( "--**--[{0}]", input );
            }

            /// <summary>
            /// Adjust the input string to be camelCase.
            /// </summary>
            /// <param name="input">The input.</param>
            /// <returns></returns>
            public static string ToCamel( string input )
            {
                return input == null ? null : input.Camelize();
            }
        }
    }
}