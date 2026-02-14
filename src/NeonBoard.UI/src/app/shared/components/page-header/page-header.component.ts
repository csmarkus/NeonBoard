import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

export interface BreadcrumbItem {
  label: string;
  link?: string[];
}

@Component({
  selector: 'app-page-header',
  imports: [RouterLink],
  templateUrl: './page-header.component.html',
})
export class PageHeaderComponent {
  breadcrumbs = input.required<BreadcrumbItem[]>();
}
