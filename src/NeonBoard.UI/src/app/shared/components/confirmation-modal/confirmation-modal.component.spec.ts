import { initTestEnvironment } from '../../../../test-setup';
import { TestBed, ComponentFixture } from '@angular/core/testing';
import { ConfirmationModalComponent } from './confirmation-modal.component';

initTestEnvironment();

describe('ConfirmationModalComponent', () => {
  let fixture: ComponentFixture<ConfirmationModalComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [ConfirmationModalComponent],
    });
    fixture = TestBed.createComponent(ConfirmationModalComponent);
    fixture.componentRef.setInput('open', true);
    fixture.componentRef.setInput('title', 'Delete item?');
    fixture.componentRef.setInput('message', 'This action cannot be undone.');
    fixture.componentRef.setInput('confirmText', 'Delete');
    fixture.componentRef.setInput('cancelText', 'Cancel');
    fixture.detectChanges();
  });

  it('renders the title and message when open', () => {
    const title = fixture.nativeElement.querySelector('h3');
    const message = fixture.nativeElement.querySelector('p');

    expect(title.textContent.trim()).toBe('Delete item?');
    expect(message.textContent.trim()).toBe('This action cannot be undone.');
  });

  it('emits confirm when the confirm button is clicked', () => {
    let confirmed = false;
    fixture.componentInstance.confirm.subscribe(() => (confirmed = true));

    const buttons: HTMLButtonElement[] = Array.from(fixture.nativeElement.querySelectorAll('button'));
    const confirmBtn = buttons.find(b => b.textContent?.trim() === 'Delete');
    confirmBtn?.click();

    expect(confirmed).toBe(true);
  });

  it('emits cancel when the cancel button is clicked', () => {
    let cancelled = false;
    fixture.componentInstance.cancel.subscribe(() => (cancelled = true));

    const buttons: HTMLButtonElement[] = Array.from(fixture.nativeElement.querySelectorAll('button'));
    const cancelBtn = buttons.find(b => b.textContent?.trim() === 'Cancel');
    cancelBtn?.click();

    expect(cancelled).toBe(true);
  });
});
