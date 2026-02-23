import { initTestEnvironment } from '../../../../test-setup';
import { TestBed, ComponentFixture } from '@angular/core/testing';
import { FormsModule } from '@angular/forms';
import { Component } from '@angular/core';
import { InputComponent } from './input.component';

initTestEnvironment();

@Component({
  imports: [FormsModule, InputComponent],
  template: `<app-input [(ngModel)]="value" />`,
})
class TestHostComponent {
  value = '';
}

describe('InputComponent', () => {
  let fixture: ComponentFixture<InputComponent>;
  let component: InputComponent;

  beforeEach(() => {
    TestBed.configureTestingModule({ imports: [InputComponent] });
    fixture = TestBed.createComponent(InputComponent);
    component = fixture.componentInstance;
  });

  it('renders an input element by default', () => {
    fixture.detectChanges();
    const input = fixture.nativeElement.querySelector('input');
    const textarea = fixture.nativeElement.querySelector('textarea');
    expect(input).toBeTruthy();
    expect(textarea).toBeNull();
  });

  it('renders a textarea when element is textarea', () => {
    fixture.componentRef.setInput('element', 'textarea');
    fixture.detectChanges();
    const input = fixture.nativeElement.querySelector('input');
    const textarea = fixture.nativeElement.querySelector('textarea');
    expect(input).toBeNull();
    expect(textarea).toBeTruthy();
  });

  it('writeValue updates the displayed value', () => {
    fixture.detectChanges();
    component.writeValue('hello');
    fixture.detectChanges();
    const input: HTMLInputElement = fixture.nativeElement.querySelector('input');
    expect(input.value).toBe('hello');
  });

  it('writeValue handles null gracefully', () => {
    component.writeValue(null as unknown as string);
    expect(component.value()).toBe('');
  });

  it('onInput calls registered onChange callback', () => {
    const spy = vi.fn();
    component.registerOnChange(spy);
    fixture.detectChanges();

    const input: HTMLInputElement = fixture.nativeElement.querySelector('input');
    input.value = 'typed';
    input.dispatchEvent(new Event('input'));

    expect(spy).toHaveBeenCalledWith('typed');
  });

  it('onBlur calls registered onTouched callback', () => {
    const spy = vi.fn();
    component.registerOnTouched(spy);
    fixture.detectChanges();

    const input: HTMLInputElement = fixture.nativeElement.querySelector('input');
    input.dispatchEvent(new Event('blur'));

    expect(spy).toHaveBeenCalled();
  });

  it('setDisabledState disables the input', () => {
    fixture.detectChanges();
    component.setDisabledState(true);
    fixture.detectChanges();
    const input: HTMLInputElement = fixture.nativeElement.querySelector('input');
    expect(input.disabled).toBe(true);
  });

  it('sets maxlength attribute when provided', () => {
    fixture.componentRef.setInput('maxlength', 50);
    fixture.detectChanges();
    const input: HTMLInputElement = fixture.nativeElement.querySelector('input');
    expect(input.getAttribute('maxlength')).toBe('50');
  });

  it('includes resize-none class for textarea by default', () => {
    fixture.componentRef.setInput('element', 'textarea');
    fixture.detectChanges();
    const textarea: HTMLTextAreaElement = fixture.nativeElement.querySelector('textarea');
    expect(textarea.className).toContain('resize-none');
  });

  it('includes resize-y class when resize is y', () => {
    fixture.componentRef.setInput('element', 'textarea');
    fixture.componentRef.setInput('resize', 'y');
    fixture.detectChanges();
    const textarea: HTMLTextAreaElement = fixture.nativeElement.querySelector('textarea');
    expect(textarea.className).toContain('resize-y');
  });
});

describe('InputComponent with ngModel', () => {
  let fixture: ComponentFixture<TestHostComponent>;
  let host: TestHostComponent;

  beforeEach(() => {
    TestBed.configureTestingModule({ imports: [TestHostComponent] });
    fixture = TestBed.createComponent(TestHostComponent);
    host = fixture.componentInstance;
  });

  it('syncs value from model to view', async () => {
    fixture.detectChanges();
    await fixture.whenStable();
    host.value = 'from-model';
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
    const input: HTMLInputElement = fixture.nativeElement.querySelector('input');
    expect(input.value).toBe('from-model');
  });

  it('syncs value from view to model', async () => {
    fixture.detectChanges();
    await fixture.whenStable();
    const input: HTMLInputElement = fixture.nativeElement.querySelector('input');
    input.value = 'from-view';
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    expect(host.value).toBe('from-view');
  });
});
