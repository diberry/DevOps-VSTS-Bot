﻿// ———————————————————————————————
// <copyright file="BuildsDialog.cs">
// Licensed under the MIT License. See License.txt in the project root for license information.
// </copyright>
// <summary>
// Represents the dialog to list/queue builds.
// </summary>
// ———————————————————————————————
namespace Vsar.TSBot.Dialogs
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Cards;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;

    /// <summary>
    /// Represents the dialog to list/queue builds.
    /// </summary>
    [CommandMetadata("builds")]
    [Serializable]
    public class BuildsDialog : IDialog<object>
    {
        private const string CommandMatchBuilds = "builds";

        [NonSerialized]
        private IVstsService vstsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildsDialog"/> class.
        /// </summary>
        /// <param name="vstsService">VSTS accessor</param>
        /// <exception cref="ArgumentNullException">Occurs when the vstsService is missing.</exception>
        public BuildsDialog(IVstsService vstsService)
        {
            if (vstsService == null)
            {
                throw new ArgumentNullException(nameof(vstsService));
            }

            this.vstsService = vstsService;
        }

        /// <summary>
        /// Gets or sets the account.
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// Gets or sets the profile.
        /// </summary>
        public VstsProfile Profile { get; set; }

        /// <summary>
        /// Gets or sets the Team Project.
        /// </summary>
        public string TeamProject { get; set; }

        /// <inheritdoc />
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.BuildsAsync);

            await Task.CompletedTask;
        }

        /// <summary>
        /// Gets a list of builds.
        /// </summary>
        /// <param name="context">A <see cref="IDialogContext"/>.</param>
        /// <param name="result">A <see cref="IMessageActivity"/>/</param>
        /// <returns>A <see cref="Task"/>.</returns>
        public async Task BuildsAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            this.Account = context.UserData.GetAccount();
            this.Profile = context.UserData.GetProfile();
            this.TeamProject = context.UserData.GetTeamProject();

            var activity = await result;
            var text = (activity.Text ?? string.Empty).ToLowerInvariant();
            var reply = context.MakeMessage();

            if (text.Equals(CommandMatchBuilds, StringComparison.OrdinalIgnoreCase))
            {
                var buildDefinitions = await this.vstsService.GetBuildDefinitionsAsync(this.TeamProject, this.Account, this.Profile.Token);
                var cards = buildDefinitions.Select(bd => new BuildDefinitionCard(bd)).ToList();

                foreach (var card in cards)
                {
                    reply.Attachments.Add(card);
                }

                reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                await context.PostAsync(reply);
            }

            context.Done(reply);
        }

        [ExcludeFromCodeCoverage]
        [OnSerializing]
        private void OnSerializingMethod(StreamingContext context)
        {
            this.vstsService = GlobalConfiguration.Configuration.DependencyResolver.GetService<IVstsService>();
        }
    }
}