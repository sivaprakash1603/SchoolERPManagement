import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Sidebar } from '../sidebar/sidebar';
import { Header } from '../header/header';
import { Footer } from '../footer/footer';
import { LayoutService } from '../../services/layout.service';
import { ToastService } from '../../services/toast.service';
import * as signalR from '@microsoft/signalr';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [RouterOutlet, Sidebar, Header, Footer],
  templateUrl: './layout.html',
  styleUrl: './layout.css',
})
export class Layout implements OnInit, OnDestroy {
  layoutService = inject(LayoutService);
  toastService = inject(ToastService);
  private hubConnection?: signalR.HubConnection;

  ngOnInit() {
    const token = sessionStorage.getItem('token');
    if (!token) return;

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:5203/hubs/notification', {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start()
      .then(() => console.log('SignalR NotificationHub connected.'))
      .catch(err => console.error('Error starting SignalR connection:', err));

    this.hubConnection.on('ReceiveNotification', (notification: any) => {
      this.toastService.info(`${notification.title}: ${notification.message}`);
    });
  }

  ngOnDestroy() {
    if (this.hubConnection) {
      this.hubConnection.stop()
        .then(() => console.log('SignalR NotificationHub disconnected.'))
        .catch(err => console.error('Error stopping SignalR connection:', err));
    }
  }
}
