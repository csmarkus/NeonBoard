import { initTestEnvironment } from '../../../test-setup';
import { TestBed, ComponentFixture } from '@angular/core/testing';
import { signal } from '@angular/core';
import { provideRouter } from '@angular/router';
import { AuthService } from '@auth0/auth0-angular';
import { SidebarComponent } from './sidebar.component';
import { SidebarStateFacade } from './services/sidebar-state.facade';

initTestEnvironment();

describe('SidebarComponent', () => {
  let fixture: ComponentFixture<SidebarComponent>;
  let component: SidebarComponent;
  let mockAuth: { logout: ReturnType<typeof vi.fn> };
  let mockFacade: {
    collapsed: ReturnType<typeof signal<boolean>>;
    userMenuOpen: ReturnType<typeof signal<boolean>>;
    boardsMenuOpen: ReturnType<typeof signal<boolean>>;
    boards: ReturnType<typeof signal<never[]>>;
    sidebarClasses: ReturnType<typeof signal<string>>;
    collapseButtonClasses: ReturnType<typeof signal<string>>;
    userButtonClasses: ReturnType<typeof signal<string>>;
    userMenuClasses: ReturnType<typeof signal<string>>;
    toggleCollapsed: ReturnType<typeof vi.fn>;
    toggleUserMenu: ReturnType<typeof vi.fn>;
    closeUserMenu: ReturnType<typeof vi.fn>;
    toggleBoardsMenu: ReturnType<typeof vi.fn>;
    isBoardActive: ReturnType<typeof vi.fn>;
  };

  beforeEach(() => {
    mockAuth = { logout: vi.fn() };
    mockFacade = {
      collapsed: signal(false),
      userMenuOpen: signal(false),
      boardsMenuOpen: signal(false),
      boards: signal([]),
      sidebarClasses: signal(''),
      collapseButtonClasses: signal(''),
      userButtonClasses: signal(''),
      userMenuClasses: signal(''),
      toggleCollapsed: vi.fn(),
      toggleUserMenu: vi.fn(),
      closeUserMenu: vi.fn(),
      toggleBoardsMenu: vi.fn(),
      isBoardActive: vi.fn().mockReturnValue(false),
    };

    TestBed.configureTestingModule({
      imports: [SidebarComponent],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: mockAuth },
        { provide: SidebarStateFacade, useValue: mockFacade },
      ],
    });
    TestBed.overrideTemplate(SidebarComponent, '');

    fixture = TestBed.createComponent(SidebarComponent);
    component = fixture.componentInstance;
  });

  it('logout calls auth.logout with returnTo: window.location.origin', () => {
    fixture.componentRef.setInput('projectId', 'p-1');
    fixture.componentRef.setInput('shortId', 's-1');
    component.logout();

    expect(mockAuth.logout).toHaveBeenCalledWith({
      logoutParams: { returnTo: window.location.origin },
    });
  });
});
