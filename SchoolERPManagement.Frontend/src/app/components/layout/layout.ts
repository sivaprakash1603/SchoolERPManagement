import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Sidebar } from '../sidebar/sidebar';
import { Header } from '../header/header';
import { Footer } from '../footer/footer';
import { LayoutService } from '../../services/layout.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [RouterOutlet, Sidebar, Header, Footer],
  templateUrl: './layout.html',
  styleUrl: './layout.css',
})
export class Layout {
  layoutService = inject(LayoutService);
  toastService = inject(ToastService);
}
