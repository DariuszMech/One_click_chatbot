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
                LocationStepAsync,
                DestinationStepAsync,
                SummaryStepAsync,
                ConfirmStepAsync,
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt), ValidateNameAsync)); // For name validation
            AddDialog(new TextPrompt(nameof(TextPrompt) + "Location", ValidateLocationAsync)); // For location validation
            AddDialog(new TextPrompt(nameof(TextPrompt) + "Destination", ValidateDestinationAsync)); // For destination validation
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

        private async Task<DialogTurnResult> LocationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["name"] = (string)stepContext.Result; // Store name
            return await stepContext.PromptAsync(nameof(TextPrompt) + "Location", new PromptOptions
            {
                Prompt = MessageFactory.Text("Please enter Your location:")  // Asking for location
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> DestinationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["location"] = (string)stepContext.Result; // Store location
            return await stepContext.PromptAsync(nameof(TextPrompt) + "Destination", new PromptOptions
            {
                Prompt = MessageFactory.Text("Please enter Your destination:") 
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["destination"] = (string)stepContext.Result; // Store destination

            // Get the current profile object from user state.
            var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            userProfile.Name = (string)stepContext.Values["name"];
            userProfile.Location = (string)stepContext.Values["location"];
            userProfile.Destination = (string)stepContext.Values["destination"];

            var msg = $"I have Your name as {userProfile.Name}, location: {userProfile.Location}, destination: {userProfile.Destination}.";
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("Is collected data correct?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var msg = $"";

            if ((bool)stepContext.Result)
            {
                msg += $"Your profile was saved successfully.";
            }
            else
            {
                msg += $"Your profile was not saved.";
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

        // Validation function for the location step
        private async Task<bool> ValidateLocationAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var location = promptContext.Recognized.Value;

            // Checks
            if (location.Length > 300)
            {
                await promptContext.Context.SendActivityAsync(
                    MessageFactory.Text("Location is too long!"),
                    cancellationToken);
                return false;
            }

            return true;
        }

        // Validation function for the destination step
        private async Task<bool> ValidateDestinationAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var destination = promptContext.Recognized.Value;

            // Checks
            if (destination.Length > 300)
            {
                await promptContext.Context.SendActivityAsync(
                    MessageFactory.Text("Destination is too long!"),
                    cancellationToken);
                return false;
            }

            return true;
        }
    }
}
