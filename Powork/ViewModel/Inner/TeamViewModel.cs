﻿using CommunityToolkit.Mvvm.ComponentModel;
using Powork.Model;
using Powork.Repository;
using System.Windows.Forms;
using System.Windows.Media;

namespace Powork.ViewModel.Inner
{
    internal class TeamViewModel : ObservableObject
    {
        public TeamViewModel()
        {
        }
        public TeamViewModel(Team team)
        {
            this.ID = team.ID;
            this.Name = team.Name;

            List<User> memberList = TeamRepository.SelectTeamMember(ID);
            foreach (User user in memberList)
            {
                if (MemberList != "")
                {
                    MemberList += "\n";
                }
                MemberList += user.Name;
            }
        }
        public string ID { get; set; }
        public string Name { get; set; }

        private bool selected;
        public bool Selected
        { 
            get
            {
                return selected;
            }
            set
            {
                selected = value;
                if (selected)
                {
                    BackgroundColor = new SolidColorBrush(Color.FromRgb(100, 100, 100));
                }
                else
                {
                    BackgroundColor = new SolidColorBrush(Color.FromRgb(39, 39, 39));
                }
            }
        }
        private Brush backgroundColor = new SolidColorBrush(Color.FromRgb(39, 39, 39));
        public Brush BackgroundColor
        { 
            get
            { 
                return backgroundColor;
            }
            set
            {
                SetProperty<Brush>(ref backgroundColor, value);
            }
        }

        private string memberList;
        public string MemberList
        {
            get
            {
                return memberList;
            }
            set
            {
                SetProperty<string>(ref memberList, value);
            }
        }
    }
}
