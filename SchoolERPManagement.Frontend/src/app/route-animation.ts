import {
  trigger,
  transition,
  style,
  query,
  animate,
  group,
} from '@angular/animations';

export const routeTransitionAnimation = trigger('routeAnimation', [
  transition('* <=> *', [
    // Set up: position both views
    query(':enter, :leave', [
      style({
        position: 'absolute',
        top: 0,
        left: 0,
        width: '100%',
      }),
    ], { optional: true }),

    // Animate leaving view out, entering view in
    group([
      query(':leave', [
        style({ opacity: 1, transform: 'translateY(0)' }),
        animate(
          '280ms cubic-bezier(0.4, 0, 0.2, 1)',
          style({ opacity: 0, transform: 'translateY(-12px)' })
        ),
      ], { optional: true }),

      query(':enter', [
        style({ opacity: 0, transform: 'translateY(12px)' }),
        animate(
          '350ms 80ms cubic-bezier(0.0, 0.0, 0.2, 1)',
          style({ opacity: 1, transform: 'translateY(0)' })
        ),
      ], { optional: true }),
    ]),
  ]),
]);
