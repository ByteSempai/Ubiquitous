/*
	FluorineFx open source library 
	Copyright (C) 2007 Zoltan Csibi, zoltan@TheSilentGroup.com, FluorineFx.com 
	
	This library is free software; you can redistribute it and/or
	modify it under the terms of the GNU Lesser General Public
	License as published by the Free Software Foundation; either
	version 2.1 of the License, or (at your option) any later version.
	
	This library is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
	Lesser General Public License for more details.
	
	You should have received a copy of the GNU Lesser General Public
	License along with this library; if not, write to the Free Software
	Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/
using System;
using dotFlex.Configuration;
using dotFlex.Messaging.Api;
using dotFlex.Messaging.Endpoints;
using dotFlex.Messaging.Rtmp.Stream;

namespace dotFlex.Messaging
{
	/// <summary>
    /// The global scope that acts as root for all applications in a host.
	/// </summary>
	class GlobalScope : Scope, IGlobalScope
	{
        internal GlobalScope()
		{
		}


		#region IGlobalScope Members

		public void Register()
		{
            //Start services
            dotFlex.Messaging.Rtmp.IO.IStreamableFileFactory streamableFileFactory = ObjectFactory.CreateInstance(FluorineConfiguration.Instance.FluorineSettings.StreamableFileFactory.Type) as dotFlex.Messaging.Rtmp.IO.IStreamableFileFactory;
            AddService(typeof(dotFlex.Messaging.Rtmp.IO.IStreamableFileFactory), streamableFileFactory, false);
            streamableFileFactory.Start(null);
            dotFlex.Scheduling.SchedulingService schedulingService = new dotFlex.Scheduling.SchedulingService();
            AddService(typeof(dotFlex.Scheduling.ISchedulingService), schedulingService, false);
            schedulingService.Start(null);
            dotFlex.Messaging.Rtmp.Stream.IBWControlService bwControlService = ObjectFactory.CreateInstance(FluorineConfiguration.Instance.FluorineSettings.BWControlService.Type) as dotFlex.Messaging.Rtmp.Stream.IBWControlService;
            AddService(typeof(dotFlex.Messaging.Rtmp.Stream.IBWControlService), bwControlService, false);
            bwControlService.Start(null);
            VideoCodecFactory videoCodecFactory = new VideoCodecFactory();
            AddService(typeof(VideoCodecFactory), videoCodecFactory, false);
            Init();
		}

		#endregion
	}
}
