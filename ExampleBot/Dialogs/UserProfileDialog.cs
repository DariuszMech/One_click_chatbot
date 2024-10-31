// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Models;

namespace Dialogs
{
    public class UserProfileDialog : ComponentDialog
    {
        private readonly IStatePropertyAccessor<UserProfile> _userProfileAccessor;

        public UserProfileDialog(UserState userState)
            : base(nameof(UserProfileDialog))
        {
            _userProfileAccessor = userState.CreateProperty<UserProfile>("UserProfile");

            // This array defines how the Waterfall will execute.
            var waterfallSteps = new WaterfallStep[]
            {
                NameStepAsync,
                Org_nameStepAsync,
                LocationStepAsync,
                SummaryStepAsync,
                ConfirmStepAsync,
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt), ValidateNameAsync)); // For name validation
            AddDialog(new TextPrompt(nameof(TextPrompt) + "Org_name", ValidateOrg_nameAsync)); // For Org_name validation
            AddDialog(new TextPrompt(nameof(TextPrompt) + "Location", ValidateLocationAsync)); // For Location validation
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("Please enter Your first name:")  // Asking for name
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> Org_nameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["name"] = (string)stepContext.Result; // Store name
            return await stepContext.PromptAsync(nameof(TextPrompt) + "Org_name", new PromptOptions
            {
                Prompt = MessageFactory.Text("Please enter the name of the organization involved in the data breach:")  // Asking for Org_name
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> LocationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["Org_name"] = (string)stepContext.Result; // Store Org_name
            return await stepContext.PromptAsync(nameof(TextPrompt) + "Location", new PromptOptions
            {
                Prompt = MessageFactory.Text("Please enter Your Location:") 
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["Location"] = (string)stepContext.Result; // Store Location

            // Get the current profile object from user state.
            var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            userProfile.Name = (string)stepContext.Values["name"];
            userProfile.Org_name = (string)stepContext.Values["Org_name"];
            userProfile.Location = (string)stepContext.Values["Location"];

            var msg = "Thank you. Just to confirm, I have the following details \n" +
            $"Your name: {userProfile.Name},\n" + 
            $"Org_name: {userProfile.Org_name},\n" +
            $"Location: {userProfile.Location}";

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("Is collected data correct?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var msg = $"";

            if ((bool)stepContext.Result)
            {
                msg += $"Your report was saved successfully.";
            }
            else
            {
                msg += $"Your report was not saved.";
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        // Validation function for the name step
        private async Task<bool> ValidateNameAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var name = promptContext.Recognized.Value;

            // Checks
            if (name.Length > 35)
            {
                await promptContext.Context.SendActivityAsync(
                    MessageFactory.Text("Your name is too long!"),
                    cancellationToken);
                return false;
            }

            if (name.Any(char.IsDigit))
            {
                await promptContext.Context.SendActivityAsync(
                    MessageFactory.Text("Your name cannot contain numbers!"),
                    cancellationToken);
                return false;
            }

            if (name.Any(char.IsPunctuation))
            {
                await promptContext.Context.SendActivityAsync(
                    MessageFactory.Text("Your name cannot contain punctuation!"),
                    cancellationToken);
                return false;
            }

            return true;
            
        }

        // Validation function for the Org_name step
        private async Task<bool> ValidateOrg_nameAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var Org_name = promptContext.Recognized.Value;

            // Checks
            if (Org_name.Length > 300)
            {
                await promptContext.Context.SendActivityAsync(
                    MessageFactory.Text("Org_name is too long!"),
                    cancellationToken);
                return false;
            }

            return true;
        }

        // Validation function for the Location step
        private async Task<bool> ValidateLocationAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var Location = promptContext.Recognized.Value;

            // Checks
            if (Location.Length > 300)
            {
                await promptContext.Context.SendActivityAsync(
                    MessageFactory.Text("Location is too long!"),
                    cancellationToken);
                return false;
            }

            return true;
        }
    }
}
