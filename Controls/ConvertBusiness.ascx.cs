﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Plugins.com_shepherdchurch.Misc
{
    [DisplayName( "Convert Business" )]
    [Category( "Shepherd Church > Misc" )]
    [Description( "Allows you to convert a Person record into a Business and vice-versa." )]

    public partial class ConvertBusiness : RockBlock
    {
        #region Base Method Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Initialize basic information about the page structure and setup the default content.
        /// </summary>
        /// <param name="sender">Object that is generating this event.</param>
        /// <param name="e">Arguments that describe this event.</param>
        protected void Page_Load( object sender, EventArgs e )
        {
            if ( !IsPostBack )
            {
            }
        }

        #endregion

        #region Core Methods

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
        }

        /// <summary>
        /// Handles the SelectPerson event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ppSource_SelectPerson( object sender, EventArgs e )
        {
            nbSuccess.Text = string.Empty;
            nbWarning.Text = string.Empty;
            nbError.Text = string.Empty;
            pnlToBusiness.Visible = false;
            pnlToPerson.Visible = false;
            
            if ( ppSource.PersonId.HasValue )
            {
                var person = new PersonService( new RockContext() ).Get( ppSource.PersonId.Value );

                if ( person.RecordTypeValue.Guid == Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() )
                {
                    pnlToPerson.Visible = true;
                    tbPersonFirstName.Text = string.Empty;
                    tbPersonLastName.Text = person.LastName;
                    dvpPersonConnectionStatus.DefinedTypeId = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS.AsGuid() ).Id;
                }
                else
                {
                    //
                    // Ensure person record is a member of only one family.
                    //
                    var families = person.GetFamilies();
                    if ( families.Count() != 1 )
                    {
                        nbError.Text = "Cannot convert person record to a business. Please adjust the family membership of the selected person so they are only a member of a single family and then try again.";
                        return;
                    }

                    //
                    // Ensure person record is only record in family.
                    //
                    var family = families.First();
                    if ( family.Members.Count != 1 )
                    {
                        nbError.Text = "Cannot convert person record to a business. Please remove extra family members before trying to convert this person to a business.";
                        return;
                    }

                    //
                    // Ensure giving group is correct.
                    //
                    if ( person.GivingGroup == null || person.GivingGroup.Members.Count != 1 || person.GivingLeaderId != person.Id )
                    {
                        nbError.Text = "Cannot convert person record to a business. Please fix the giving group and then try again.";
                        return;
                    }

                    pnlToBusiness.Visible = true;
                    tbBusinessName.Text = string.Format( "{0} {1}", person.FirstName, person.LastName ).Trim();
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbToPersonSave_Click( object sender, EventArgs e )
        {
            var context = new RockContext();
            var person = new PersonService( context ).Get( ppSource.PersonId.Value );
            var changes = new List<string>();

            person.RecordTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON ).Id;

            History.EvaluateChange( changes, "Connection Status", DefinedValueCache.GetName( person.ConnectionStatusValueId ), DefinedValueCache.GetName( dvpPersonConnectionStatus.SelectedValueAsInt() ) );
            person.ConnectionStatusValueId = dvpPersonConnectionStatus.SelectedValueAsInt();

            History.EvaluateChange( changes, "First Name", person.FirstName, tbPersonFirstName.Text.Trim() );
            person.FirstName = tbPersonFirstName.Text.Trim();

            History.EvaluateChange( changes, "Nick Name", person.NickName, tbPersonFirstName.Text.Trim() );
            person.NickName = tbPersonFirstName.Text.Trim();

            History.EvaluateChange( changes, "Last Name", person.LastName, tbPersonLastName.Text.Trim() );
            person.LastName = tbPersonLastName.Text.Trim();

            context.SaveChanges();
            if ( changes.Count > 0 )
            {
                HistoryService.SaveChanges( context, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(), person.Id, changes );
            }

            ppSource.SetValue( null );
            nbSuccess.Text = string.Format( "{0} has been converted to a person.", person.FullName );

            pnlToPerson.Visible = false;
        }

        /// <summary>
        /// Handles the Click event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbToBusinessSave_Click( object sender, EventArgs e )
        {
            var context = new RockContext();
            var person = new PersonService( context ).Get( ppSource.PersonId.Value );
            var changes = new List<string>();

            person.RecordTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS ).Id;

            History.EvaluateChange( changes, "Connection Status", DefinedValueCache.GetName( person.ConnectionStatusValueId ), DefinedValueCache.GetName( null ) );
            person.ConnectionStatusValueId = null;

            History.EvaluateChange( changes, "Title", DefinedValueCache.GetName( person.TitleValueId ), DefinedValueCache.GetName( null ) );
            person.TitleValueId = null;

            History.EvaluateChange( changes, "First Name", person.FirstName, null );
            person.FirstName = null;

            History.EvaluateChange( changes, "Nick Name", person.NickName, null );
            person.NickName = null;

            History.EvaluateChange( changes, "Middle Name", person.MiddleName, null );
            person.MiddleName = null;

            History.EvaluateChange( changes, "Last Name", person.LastName, tbBusinessName.Text.Trim() );
            person.LastName = tbBusinessName.Text.Trim();

            History.EvaluateChange( changes, "Suffix", DefinedValueCache.GetName( person.SuffixValueId ), DefinedValueCache.GetName( null ) );
            person.SuffixValueId = null;

            History.EvaluateChange( changes, "Birth Month", person.BirthMonth, null );
            History.EvaluateChange( changes, "Birth Day", person.BirthDay, null );
            History.EvaluateChange( changes, "Birth Year", person.BirthYear, null );
            person.SetBirthDate( null );

            History.EvaluateChange( changes, "Gender", person.Gender, Gender.Unknown );
            person.Gender = Gender.Unknown;

            History.EvaluateChange( changes, "Marital Status", DefinedValueCache.GetName( person.MaritalStatusValueId ), DefinedValueCache.GetName( null ) );
            person.MaritalStatusValueId = null;

            History.EvaluateChange( changes, "Anniversary Date", person.AnniversaryDate, null );
            person.AnniversaryDate = null;

            History.EvaluateChange( changes, "Graduation Year", person.GraduationYear, null );
            person.GraduationYear = null;

            //
            // Check address(es) and make sure one is of type Work.
            //
            var family = person.GetFamily( context );
            if ( family.GroupLocations.Count > 0 )
            {
                var workLocationTypeId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_WORK ).Id;
                var homeLocationTypeId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME ).Id;

                var workLocation = family.GroupLocations.Where( gl => gl.GroupLocationTypeValueId == workLocationTypeId ).FirstOrDefault();
                if ( workLocation == null )
                {
                    var homeLocation = family.GroupLocations.Where( gl => gl.GroupLocationTypeValueId == homeLocationTypeId ).FirstOrDefault();
                    if ( homeLocation != null )
                    {
                        homeLocation.GroupLocationTypeValueId = workLocationTypeId;
                    }
                }
            }

            //
            // Check phone(es) and make sure one is of type Work.
            //
            if ( person.PhoneNumbers.Count > 0 )
            {
                var workPhoneTypeId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK ).Id;
                var homePhoneTypeId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME ).Id;

                var workPhone = person.PhoneNumbers.Where( pn => pn.NumberTypeValueId == workPhoneTypeId ).FirstOrDefault();
                if ( workPhone == null )
                {
                    var homePhone = person.PhoneNumbers.Where( pn => pn.NumberTypeValueId == homePhoneTypeId ).FirstOrDefault();
                    if ( homePhone != null )
                    {
                        homePhone.NumberTypeValueId = workPhoneTypeId;
                    }
                }
            }

            //
            // Make sure member status in family is set to Adult.
            //
            var adultRoleId = GroupTypeCache.GetFamilyGroupType().Roles
                .Where( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() )
                .Select( a => a.Id ).First();
            family.Members.Where( m => m.PersonId == person.Id ).First().GroupRoleId = adultRoleId;

            context.SaveChanges();
            if ( changes.Count > 0 )
            {
                HistoryService.SaveChanges( context, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(), person.Id, changes );
            }

            ppSource.SetValue( null );
            nbSuccess.Text = string.Format( "{0} has been converted to a business.", person.LastName );

            pnlToBusiness.Visible = false;
        }

        #endregion
    }
}