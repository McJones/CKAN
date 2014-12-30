﻿using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using CommandLine;
using CKAN;

namespace CKAN.CmdLine
{
	public struct RepositoryList
	{
		public Repository[] repositories;
	}

	public class Repo : ISubCommand
	{
        public KSPManager Manager { get; set; }
        public IUser User { get; set; }
        public string option;
        public object suboptions;

        // TODO Change the URL base to api.ksp-ckan.org
        public static readonly Uri default_repo_master_list = new Uri("http://munich.ksp-ckan.org/repositories.json");

        public Repo(KSPManager manager, IUser user)
        {
            Manager = manager;
            User = user;
        }

        internal class RepoSubOptions : CommonOptions
        {
            [VerbOption("available", HelpText="List (canonical) available repositories")]
            public CommonOptions AvailableOptions { get; set; }

            [VerbOption("list", HelpText="List repositories")]
            public CommonOptions ListOptions { get; set; }

            [VerbOption("add", HelpText="Add a repository")]
            public AddOptions AddOptions { get; set; }

            [VerbOption("forget", HelpText="Forget a repository")]
            public ForgetOptions ForgetOptions { get; set; }

            [VerbOption("default", HelpText="Set the default repository")]
            public DefaultOptions DefaultOptions { get; set; }
        }

        internal class AvailableOptions : CommonOptions
        {
        }

        internal class ListOptions : CommonOptions
        {
            User.RaiseMessage("Listing all known repositories:");
            RegistryManager manager = RegistryManager.Instance(CurrentInstance);
            Dictionary<string, Uri> repositories = manager.registry.Repositories;

            int maxNameLen = 0;
            foreach(KeyValuePair<string, Uri> repository in repositories)
            {
                maxNameLen = Math.Max(maxNameLen, repository.Key.Length);
            }

            foreach(KeyValuePair<string, Uri> repository in repositories)
            {
                User.RaiseMessage("  {0}: {1}", repository.Key.PadRight(maxNameLen), repository.Value);
            }

            return Exit.OK;
        }

        internal class AddOptions : CommonOptions
        {
            [ValueOption(0)]
            public string name { get; set; }

            [ValueOption(1)]
            public string uri { get; set; }
        }

        internal class DefaultOptions : CommonOptions
        {
            [ValueOption(0)]
            public string uri { get; set; }
        }

        internal class ForgetOptions : CommonOptions
        {
            [ValueOption(0)]
            public string name { get; set; }
        }

        internal void Parse(string option, object suboptions)
        {
            this.option = option;
            this.suboptions = suboptions;
        }

        // This is required by ISubCommand
		public int RunSubCommand(SubCommandOptions unparsed)
        {
            string[] args = unparsed.options.ToArray();

            if (args.Length == 0)
            {
                // There's got to be a better way of showing help...
                args = new string[1];
                args[0] = "help";
            }

            #region Aliases

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "remove":
                        args[i] = "forget";
                        break;

                    default:
                        break;
                }
            }

            #endregion
            // Parse and process our sub-verbs
            Parser.Default.ParseArgumentsStrict(args, new RepoSubOptions (), Parse);

            // That line above will have set our 'option' and 'suboption' fields.

            switch (option)
            {
                case "available":
                    return AvailableRepositories();

                case "list":
                    return ListRepositories();

                case "add":
                    return AddRepository((AddOptions)suboptions);

                case "forget":
                    return ForgetRepository((ForgetOptions)suboptions);

                case "default":
                    return DefaultRepository((DefaultOptions)suboptions);

                default:
                    User.RaiseMessage("Unknown command: repo {0}", option);
                    return Exit.BADOPT;
            }
        }

        public static RepositoryList FetchMasterRepositoryList(Uri master_uri = null)
        {
            WebClient client = new WebClient();

            if (master_uri == null)
            {
                master_uri = default_repo_master_list;
            }

            string json = client.DownloadString(master_uri);
            return JsonConvert.DeserializeObject<RepositoryList>(json);
        }

        private int AvailableRepositories()
        {
            User.RaiseMessage("Listing all (canonical) available CKAN repositories:");
            RepositoryList repositories = new RepositoryList();

            try
            {
                repositories = FetchMasterRepositoryList();
            }
            catch
            {
                User.RaiseError("Couldn't fetch CKAN repositories master list from {0}", Repo.default_repo_master_list.ToString());
                return Exit.ERROR;
            }

            int maxNameLen = 0;
            foreach (Repository repository in repositories.repositories)
            {
                maxNameLen = Math.Max(maxNameLen, repository.name.Length);
            }

            foreach (Repository repository in repositories.repositories)
            {
                User.RaiseMessage("  {0}: {1}", repository.name.PadRight(maxNameLen), repository.uri);
            }

            return Exit.OK;
        }

        private int ListRepositories()
        {
            return Exit.OK;
        }

        private int AddRepository(AddOptions options)
        {
            return Exit.OK;
        }

        private int ForgetRepository(ForgetOptions options)
        {
            return Exit.OK;
        }

        private int DefaultRepository(DefaultOptions options)
        {
            return Exit.OK;
        }
	}
}

