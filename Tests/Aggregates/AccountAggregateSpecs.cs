﻿using System;
using CQRS.Sample.Aggregates;
using CQRS.Sample.Commands;
using CQRS.Sample.Events;
using Machine.Specifications;
using NUnit.Framework;

namespace CQRS.Sample.Tests.Aggregates
{
    [Subject(typeof (AccountAggregate))]
    public class when_create_account_requested : AggregateContext
    {
        Establish context = () => Bus.Subscribe<AccountCreated>(OnAccountCreated);

        static string Email;
        static string Hash;

        public static void OnAccountCreated(AccountCreated msg)
        {
            Email = msg.Email;
            Hash = msg.PasswordHash;
        }

        Because of = () => CreateAccount("em@ai.il", "Swordfish");

        It should_set_email = () => Assert.That(Email, Is.EqualTo("em@ai.il"));
        It should_set_password_hash = () => Assert.True(PasswordHash.ValidatePassword("Swordfish", Hash));
    }


    [Subject(typeof(AccountAggregate))]
    public class when_password_change_requested_and_old_password_wrong: AggregateContext
    {
        protected static Guid AccountStreamID;
        protected static bool PasswordChanged;
        protected static bool FailureNotificationReceived;

        static void OnPasswordChanged(PasswordChanged msg)
        {
            PasswordChanged = true;
        }

        static void OnAccountChangeFailed(AccountChangeFailed obj)
        {
            FailureNotificationReceived = true;
        }

        Establish context = () =>
        {
            AccountStreamID = CreateAccount("em@ai.il", "Swordfish");
            Bus.Subscribe<PasswordChanged>(OnPasswordChanged);
            Bus.Subscribe<AccountChangeFailed>(OnAccountChangeFailed);
        };

        Because of = () =>
        {
            Bus.Publish(new ChangePassword(AccountStreamID)
            {
                OldPassword = "Sword",
                NewPassword = "fish",
            });
            Bus.Commit();
        };

        It should_not_change_password = () => Assert.False(PasswordChanged);
        It should_notify_that_change_failed = () => Assert.True(FailureNotificationReceived);
    }

    [Subject(typeof(AccountAggregate))]
    public class when_password_change_requested: AggregateContext
    {
        protected static Guid AccountStreamID;
        protected static string NewPasswordHash;

        static void OnPasswordChanged(PasswordChanged msg)
        {
            NewPasswordHash = msg.PasswordHash;
        }

        Establish context = () =>
        {
            AccountStreamID = CreateAccount("em@ai.il", "Swordfish");
            Bus.Subscribe<PasswordChanged>(OnPasswordChanged);
        };

        Because of = () =>
        {
            Bus.Publish(new ChangePassword(AccountStreamID)
            {
                OldPassword = "Swordfish",
                NewPassword = "fish",
            });
            Bus.Commit();
        };

        It should_have_new_password = () => Assert.True(PasswordHash.ValidatePassword("fish", NewPasswordHash));
    }

}